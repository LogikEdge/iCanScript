using UnityEngine;
using UnityEditor;
using System.Collections;

public class iCS_PortNameEditor : iCS_ISubEditor {
    // ======================================================================
    // Field.
	// ----------------------------------------------------------------------
    iCS_EditorObject    myTarget  = null;
	iCS_FieldEditor	    myEditor  = null;
	iCS_Graphics		myGraphics= null;
	
    // ======================================================================
    // Property.
	// ----------------------------------------------------------------------
	Rect 	 Position { get { return myGraphics.GetPortNameGUIPosition(myTarget, myTarget.IStorage); }}
	GUIStyle GuiStyle { get { return myGraphics.LabelStyle; }}

    // ======================================================================
    // Initialization.
	// ----------------------------------------------------------------------
    public iCS_PortNameEditor(iCS_EditorObject target, iCS_Graphics graphics, Vector2 pickPoint) {
        myTarget= target;
		myGraphics= graphics;
		myEditor= new iCS_FieldEditor(Position, myTarget.RawName, iCS_FieldTypeEnum.String, GuiStyle, pickPoint);
    }
    
    // ======================================================================
    // Update.
	// ----------------------------------------------------------------------
    public bool Update() {
		myEditor.Position= Position;
		if(myEditor.Update()) {
			iCS_UserCommands.ChangeName(myTarget, myEditor.ValueAsString);
			return true;
		}
		return false;
    }
}
