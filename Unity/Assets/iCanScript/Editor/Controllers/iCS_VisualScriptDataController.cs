using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;

public static class iCS_VisualScriptDataController {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    static bool             myIsPlaying       = false;
	static iCS_IStorage 	myIStorage        = null;
	
    // =================================================================================
    // Properties
    // ---------------------------------------------------------------------------------
    public static iCS_IStorage          IStorage         { get { return myIStorage; }}
    public static iCS_VisualScriptData  Storage          { get { return IStorage != null ? IStorage.Storage : null; }}
    public static iCS_EditorObject      SelectedObject   {
        get { return IStorage != null ? IStorage.SelectedObject : null; }
        set {
            if(IStorage != null) {
                iCS_UserCommands.Select(value, IStorage);                
            }
        }
    }
	
    // =================================================================================
    // Install
    // ---------------------------------------------------------------------------------
    static iCS_VisualScriptDataController() {
        EditorApplication.update+= PeriodicUpdate;
    }
    public static void Start() {}
    public static void Shutdown() {}
    
    // ---------------------------------------------------------------------------------
    public static bool IsSameVisualScript(iCS_IStorage iStorage, iCS_VisualScriptData storage) {
        if(iStorage == null || storage == null) return false;
        if(iStorage.Storage == storage) return true;
        return false;
    }
    // ---------------------------------------------------------------------------------
    public static bool IsSameVisualScript(iCS_MonoBehaviourImp monoBehaviour, iCS_IStorage iStorage) {
        if(monoBehaviour == null || iStorage == null) return false;
        if(iStorage.iCSMonoBehaviour == monoBehaviour) return true;
        return false;
    }

    // =================================================================================
    // Storage & Selected object Update.  This update is called by the Editors.
    // ---------------------------------------------------------------------------------
	public static void Update() {
        // Use previous game object if new selection does not include a visual script.
		GameObject go= Selection.activeGameObject;
        var monoBehaviour= go != null ? go.GetComponent<iCS_MonoBehaviourImp>() : null;
        if(monoBehaviour == null) {
            // Clear if previous game object is not valid.
                myIStorage= null;
                myIsPlaying= Application.isPlaying;
                return;                
        }
		// Verify for storage change.
        bool isPlaying= Application.isPlaying;
		if(myIStorage == null || myIStorage.iCSMonoBehaviour != monoBehaviour || myIsPlaying != isPlaying) {
            myIsPlaying= isPlaying;
			myIStorage= new iCS_IStorage(monoBehaviour);
			return;
		}
	}

    // =================================================================================
    // Periodic update called 100 times per seconds.
    // ---------------------------------------------------------------------------------
    public static void PeriodicUpdate() {
    }
}
