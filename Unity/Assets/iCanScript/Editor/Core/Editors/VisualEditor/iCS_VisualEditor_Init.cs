using UnityEngine;
using System.Collections;

public partial class iCS_VisualEditor : iCS_EditorBase {
    // ======================================================================
    // Properties.
	// ----------------------------------------------------------------------
    iCS_IStorage    myPreviousIStorage= null;

    // ======================================================================
    // Initialization
	// ----------------------------------------------------------------------
    // Prepares the editor for editing a graph.  Note that the graph to edit
    // is not configured at this point.  We must wait for an activate from
    // the graph inspector to know which graph to edit. 
	public override void OnEnable() {        
        base.OnEnable();
        
		// Tell Unity we want to be informed of move drag events
		MyWindow.wantsMouseMove= true;

        // Create worker objects.
        myGraphics   = new iCS_Graphics();
        myDynamicMenu= new iCS_DynamicMenu();
        
        // Inspect the assemblies for components.
        if(!ourAlreadyParsed) {
            ourAlreadyParsed= true;
            iCS_Reflection.ParseAppDomain();
        }
        
        // Get snapshot for realtime clock.
        myCurrentTime= Time.realtimeSinceStartup;	    
	}

	// ----------------------------------------------------------------------
    // Releases all resources used by the iCS_Behaviour editor.
    public override void OnDisable() {
        base.OnDisable();
        
        // Release all worker objects.
        myGraphics   = null;
        myDynamicMenu= null;
		mySubEditor  = null;
    }

	// ----------------------------------------------------------------------
    // Assures proper initialization and returns true if editor is ready
    // to execute.
	bool IsInitialized() {
        // Nothing to do if we don't have a Graph to edit...
        UpdateMgr();
		if(IStorage == null) {
            DisplayRoot= null;
            myBookmark= null;
            DragType= DragTypeEnum.None;
            mySubEditor= null;
		    return false;
		}
        if(IStorage != myPreviousIStorage) {
            myPreviousIStorage= IStorage;
            DisplayRoot= StorageRoot;
            myBookmark= null;
            DragType= DragTypeEnum.None;
            mySubEditor= null;
        }
        
		// Don't run if graphic sub-system did not initialise.
		if(iCS_Graphics.IsInitialized == false) {
            iCS_Graphics.Init(IStorage);
		}
        iCS_InstallerMgr.InstallGizmo();
        return true;
	}

}
