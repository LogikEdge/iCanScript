using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public enum iCS_FieldTypeEnum { String, Integer, Float };
public class iCS_FieldEditor : iCS_ISubEditor {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    string              myValue;
    Rect                myPosition;
    iCS_FieldTypeEnum   myFieldType;
    GUIStyle            myStyle;
	int					myCursor= 0;
    
    // =================================================================================
    // Properties
    // ---------------------------------------------------------------------------------
    public Rect     Position        { get { return myPosition; } set { myPosition= value; }}
    public GUIStyle GuiStyle        { get { return myStyle; }    set { myStyle= value; }}
    public string   ValueAsString   { get { return myValue; }}
    public long     ValueAsInteger  { get { return (long)Convert.ChangeType(myValue, typeof(long)); }}
    public float    ValueAsFloat    { get { return (float)Convert.ChangeType(myValue, typeof(float)); }}
    public object   Value       {
        get {
            switch(myFieldType) {
                case iCS_FieldTypeEnum.String: {
                    return ValueAsString;
                }
                case iCS_FieldTypeEnum.Integer: {
                    return ValueAsInteger;
                }
                case iCS_FieldTypeEnum.Float: {
                    return ValueAsFloat;
                }
            }
            return myValue;
        }
    }
    
    // =================================================================================
    // Initialization
    // ---------------------------------------------------------------------------------
    public iCS_FieldEditor(Rect position, object initialValue, iCS_FieldTypeEnum fieldType, GUIStyle guiStyle) {
        myValue    = (string)Convert.ChangeType(initialValue, typeof(string));
        myPosition = position;
        myFieldType= fieldType;
        myStyle    = guiStyle;
    }

    // =================================================================================
    // Update
    // ---------------------------------------------------------------------------------
    public bool Update() {
        Rect boxPos= new Rect(myPosition.x-2.0f, myPosition.y-1f, myPosition.width+4.0f, myPosition.height+2f);
        Color selectionColor= EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).settings.selectionColor;
        iCS_Graphics.DrawBox(boxPos, new Color(0f,0f,0f,0.25f), selectionColor, Color.white);
		boxPos.x+= 1f;
		boxPos.width-= 2f;
		boxPos.y+= 1f;
		boxPos.height-= 2f;
        iCS_Graphics.DrawBox(boxPos, Color.clear, selectionColor, Color.white);
		GUI.Label(myPosition, myValue, myStyle);
        GUI.changed= false;
//        GUI.SetNextControlName("SubEditor");
//        string newValue= GUI.TextField(myPosition, myValue, myStyle);
//		var guiStyle= new GUIStyle(EditorStyles.textField);
//		guiStyle.fontSize= myStyle.fontSize;
//		guiStyle.fontStyle= myStyle.fontStyle;
//		GUI.skin.settings.cursorColor= Color.red;
//		GUI.skin.settings.cursorFlashSpeed= 1;
//		GUI.backgroundColor= Color.clear;
//        string newValue= GUI.TextField(boxPos/*myPosition*/, myValue, guiStyle);
		ShowCursor(myPosition, myValue, myCursor, Color.red, 0.5f, myStyle);
        var oldValue= myValue;
        if(ProcessKeys(ref myValue, ref myCursor, (ch,_,__)=> !char.IsControl(ch))) {
            RestartCursorBlink();
        }
        return oldValue != myValue;
    }
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
		Handles.color= cursorColor;
		var x= r.x;
		var y= r.y-2f;
		var yMax= r.yMax+2f;
		if(cursor != 0) {
			// Limit the cursor movement within the value string
			if(cursor > value.Length) {
				cursor= value.Length;
			}
            // Avoid end-of-string space removal by appending "A".
			var beforeCursor= value.Substring(0, cursor)+"A";
			var size= style.CalcSize(new GUIContent(beforeCursor));
            var toTrim= style.CalcSize(new GUIContent("A"));
			x+= size.x-toTrim.x;
		}
		Handles.DrawLine(new Vector3(x,y,0), new Vector3(x,yMax,0));
		Handles.DrawLine(new Vector3(x+1,y,0), new Vector3(x+1,yMax,0));
	}
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
