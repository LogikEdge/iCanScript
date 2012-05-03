using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class iCS_HierarchyEditor : iCS_EditorWindow {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    DSScrollView                    myMainView;
	iCS_ObjectHierarchyController   myController;
	    
    // =================================================================================
    // Activation/Deactivation.
    // ---------------------------------------------------------------------------------
	public override void OnStorageChange() {
        if(IStorage == null) return;
        myController= new iCS_ObjectHierarchyController(IStorage[0], IStorage);
        myMainView= new DSScrollView(new RectOffset(0,0,0,0), false, myController.View);
		Repaint();
    }
    
	// =================================================================================
    // Display.
    // ---------------------------------------------------------------------------------
    void OnGUI() {
        iCS_EditorMgr.Update();
		if(IStorage == null) return;
		myMainView.Display(new Rect(0,0,position.width,position.height));
	}
}
