using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class iCS_FieldEditor : iCS_ISubEditor {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    string      				myValueAsString;
    Rect        				myPosition;
    Type   						myValueType;
    GUIStyle    				myStyle;
	int							myCursor         = 0;
	int							mySelectionStart = 0;
	int							mySelectionLength= 0;
	Color						mySelectionColor;
    float						myBackgroundAlpha= 0.25f;
	Func<char,string,int,bool>	myInputValidator = null;
	
    // =================================================================================
    // Properties
    // ---------------------------------------------------------------------------------
    public Rect     Position        { get { return myPosition; } set { myPosition= value; }}
    public GUIStyle GuiStyle        { get { return myStyle; }    set { myStyle= value; }}
    public object   Value       {
        get {
			var theValueAsString= myValueAsString;
			// Return default value for incomplete entries
			if(myValueType == typeof(float) || myValueType == typeof(int)) {
				bool containsDigit= false;
				foreach(char c in myValueAsString) {
					if(char.IsDigit(c)) {
						containsDigit= true;
					}
				}
				if(!containsDigit) {
					theValueAsString= "0";
				}
			}
			return Convert.ChangeType(theValueAsString, myValueType);
        }
    }
	static T ValueAs<T>(object value) {
		return (T)Convert.ChangeType(value, typeof(T));
	}
	
    // =================================================================================
    // Initialization
    // ---------------------------------------------------------------------------------
    public iCS_FieldEditor(Rect position, object initialValue, Type valueType, GUIStyle guiStyle, Vector2 pickPoint) {
        myValueAsString = ValueAs<string>(initialValue);
        myPosition 	    = position;
        myValueType	    = valueType;
        myStyle    	    = guiStyle;
        myCursor   	    = GetCursorIndexFromPosition(myPosition, pickPoint, myValueAsString, myStyle);
		myInputValidator= GetInputValidator(valueType);
		mySelectionColor= EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).settings.selectionColor;
    }
	public void SetBackgroundAlpha(float alpha) {
		myBackgroundAlpha= alpha;
	}
	
     // =================================================================================
    // Update
    // ---------------------------------------------------------------------------------
    public bool Update() {
		// An input validator is required.
		if(myInputValidator == null) {
			Debug.Log("iCanScript: No input validator defined to edit type=> "+myValueType);
			return false;
		}
		
        Rect boxPos= new Rect(myPosition.x-2.0f, myPosition.y-1f, myPosition.width+4.0f, myPosition.height+2f);
		// Draw the edit box
        iCS_Graphics.DrawBox(boxPos, new Color(0f,0f,0f,myBackgroundAlpha), mySelectionColor, Color.white);
		boxPos.x+= 1f;
		boxPos.width-= 2f;
		boxPos.y+= 1f;
		boxPos.height-= 2f;
        iCS_Graphics.DrawBox(boxPos, Color.clear, mySelectionColor, Color.white);
		// Draw the selection box
		if(mySelectionLength != 0) {
			// TODO: Show the selection background.
		}
		// Display text
		GUI.Label(myPosition, myValueAsString, myStyle);
		ShowCursor(myPosition, myValueAsString, myCursor, Color.red, 0.5f, myStyle);
        var oldValue= myValueAsString;
        if(ProcessKeys(ref myValueAsString, ref myCursor, myInputValidator)) {
            RestartCursorBlink();
        }
        return oldValue != myValueAsString;
    }

    // =================================================================================
    // Cursor Management
    // ---------------------------------------------------------------------------------
    static float ourCursorBlinkStartTime= 0f;
    static void RestartCursorBlink() {
        ourCursorBlinkStartTime= Time.realtimeSinceStartup;
    }
    // ---------------------------------------------------------------------------------
	static void ShowCursor(Rect r, string value, int cursor, Color cursorColor, float blinkSpeed, GUIStyle style) {
		if(Math3D.IsNotZero(blinkSpeed)) {
			int step= (int)((Time.realtimeSinceStartup-ourCursorBlinkStartTime)/blinkSpeed);
			if((step & 1) == 1) {
				return;
			}
		}
        var x= r.x+GetCursorGUIOffset(value, cursor, style);
		var y= r.y-2f;
		var yMax= r.yMax+2f;
		Handles.color= cursorColor;
		Handles.DrawLine(new Vector3(x,y,0), new Vector3(x,yMax,0));
		Handles.DrawLine(new Vector3(x+1,y,0), new Vector3(x+1,yMax,0));
	}
    // ---------------------------------------------------------------------------------
    static int GetCursorIndexFromPosition(Rect r, Vector2 pickPoint, string value, GUIStyle style) {
        if(value == null) return 0;
        var len= value.Length;
        if(len == 0) return 0;
        var x= pickPoint.x - r.x;
        var size= style.CalcSize(new GUIContent(value));
        // Determine rough estimate of cursor.
        var step= size.x/len;
        int cursor= (int)((x/step)+0.5f);
        if(cursor < 0) cursor= 0;
        if(cursor > len) cursor= len;
        // Fine tune cursor.
        var offset= GetCursorGUIOffset(value, cursor, style);
        var diff= Mathf.Abs(offset-x);
        while(cursor > 0 && offset > x) {
            --cursor;
            offset= GetCursorGUIOffset(value, cursor, style);
            var newDiff= Mathf.Abs(offset-x);
            if(newDiff > diff) {
                ++cursor;
                break;
            }
            diff= newDiff;
        }
        while(cursor < len && offset < x) {
            ++cursor;
            offset= GetCursorGUIOffset(value, cursor, style);
            var newDiff= Mathf.Abs(offset-x);
            if(newDiff > diff) {
                --cursor;
                break;
            }
            diff= newDiff;
        }
        return cursor;
    }
    // ---------------------------------------------------------------------------------
    static float GetCursorGUIOffset(string value, int cursor, GUIStyle style) {
        float x= 0f;
		if(cursor != 0) {
			// Limit the cursor movement within the value string
			if(cursor > value.Length) {
				cursor= value.Length;
			}
            // Avoid end-of-string space removal by appending "A".
			var beforeCursor= value.Substring(0, cursor)+"A";
			var size= style.CalcSize(new GUIContent(beforeCursor));
            var toTrim= style.CalcSize(new GUIContent("A"));
			x= size.x-toTrim.x;
		}
        return x;        
    }

    // =================================================================================
    // Input Validation
    // ---------------------------------------------------------------------------------
	public static Func<char,string,int,bool> GetInputValidator(Type type) {
		if(type == typeof(string)) return ValidateStringInput;
		if(type == typeof(int)) return ValidateIntInput;
		if(type == typeof(float)) return ValidateFloatInput;
		return null;
	}
    static bool ValidateStringInput(char newChar, string value, int cursor) {
        return !char.IsControl(newChar);
    }
    static bool ValidateIntInput(char newChar, string value, int cursor) {
        if(char.IsDigit(newChar)) return true;
		if(newChar == '-') {
			if(cursor != 0) return false;
			if(value.Length == 0) return true;
			if(value[0] != '-') return true;
		}
		return false;
    }
    static bool ValidateFloatInput(char newChar, string value, int cursor) {
        if(char.IsDigit(newChar)) return true;
		var len= value.Length;
		if(newChar == '-') {
			if(cursor != 0) return false;
			if(len == 0) return true;
			if(value[0] != '-') return true;			
		}
		if(newChar == '.') {
			if(cursor == 0 && len != 0 && value[0] == '-') return false;
			bool isDecimalAlreadyPresent= false;
			foreach(char c in value) {
				if(c == '.') {
					isDecimalAlreadyPresent= true;
				}
			}
			return !isDecimalAlreadyPresent;
		}
		return false;
    }
    
    // =================================================================================
    // Keyboard processing
    // ---------------------------------------------------------------------------------
    static bool ProcessKeys(ref string value, ref int cursor, Func<char,string,int,bool> filter) {
        // Nothing to do if not a keyboard event
        var ev= Event.current;
        if(ev.type == EventType.KeyDown) {
            var len= value.Length;
            switch(ev.keyCode) {
                case KeyCode.RightArrow: {
                    cursor= cursor+1;
                    if(cursor > len) {
                        cursor= len;
                    }
                    Event.current.Use();
                    return true;
                }
                case KeyCode.LeftArrow: {
                    cursor= cursor-1;
                    if(cursor < 0) {
                        cursor= 0;
                    }
                    Event.current.Use();
                    return true;;
                }
                case KeyCode.Delete:
                case KeyCode.Backspace: {
                    if(cursor > 0) {
                        value= value.Substring(0, cursor-1)+value.Substring(cursor, len-cursor);
                        --cursor;
                    }
                    Event.current.Use();
                    return true;
                }
                default: {
                    if(ev.isKey) {
                        char c= ev.character;
                        if(filter(c, value, cursor)) {
                            value= value.Substring(0, cursor)+c+value.Substring(cursor, len-cursor);
                            ++cursor;                            
                            Event.current.Use();
                            return true;
                        }
                    }
                    break;                    
                }
            }            
        }
        return false;
    }
}
