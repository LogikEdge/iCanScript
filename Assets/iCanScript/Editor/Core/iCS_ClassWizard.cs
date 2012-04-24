using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class iCS_ClassWizard : EditorWindow {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    DSAccordionView             myMainView     = null;
    iCS_ClassWizardController   myController   = null;
    DSTreeView                  myTreeView     = null;
    DSCellView                  myInspectorView= null;
    
    // =================================================================================
    // Constants
    // ---------------------------------------------------------------------------------
    const int   kSpacer= 8;
    
    // =================================================================================
    // Activation/Deactivation.
    // ---------------------------------------------------------------------------------
    public void OnActivate(iCS_EditorObject target, iCS_IStorage storage) {
        // Transform invalid activation to a deactivation.
        if(target == null || storage == null) {
            myMainView= null;
            return;
        }
        if(myMainView == null ||
           (myController != null && (myController.Target != target || myController.IStorage != storage))) {
               myController   = new iCS_ClassWizardController(target, storage);            
               myTreeView     = new DSTreeView(new RectOffset(kSpacer,kSpacer,kSpacer,kSpacer), true);
               myInspectorView= new DSCellView(new RectOffset(kSpacer,kSpacer,kSpacer,kSpacer), true);
               myMainView     = new DSAccordionView(new RectOffset(kSpacer, kSpacer, kSpacer, kSpacer), true, 3);
               myMainView.AddSubview(new GUIContent("Wizard"), myController.View);
               myMainView.AddSubview(new GUIContent("Inspector"), myInspectorView);
               myMainView.AddSubview(new GUIContent("Tree View"), myTreeView);
        }
        Repaint();
    }
    // ---------------------------------------------------------------------------------
    public void OnDeactivate() {
        myMainView= null;
        Repaint();
    }

    // =================================================================================
    // Display.
    // ---------------------------------------------------------------------------------
    void OnGUI() {
        // Wait until window is configured.
        if(myController == null) return;
        EditorGUIUtility.LookLikeInspector();
        myMainView.Display(new Rect(0,0,position.width, position.height));
    }
}
