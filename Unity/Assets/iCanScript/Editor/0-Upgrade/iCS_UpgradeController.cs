﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// Validate & Perform upgrade.
[InitializeOnLoad]
public static class iCS_UpgradeController {
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    static iCS_UpgradeController()    {
		CleanupAfterImport();
    }
    public static void Start()      {}
    public static void Shutdown() {
    }

    // ======================================================================
    // Code cleanup
    // ----------------------------------------------------------------------
	public static void CleanupAfterImport() {
		string kEditorCommunityDLL= "Assets/iCanScript/Editor/iCanScriptEditorCommunity.dll";
		string kEditorProDLL      = Application.dataPath+"/iCanScript/Editor/iCanScriptEditorPro.dll";
		if(File.Exists(kEditorProDLL)) {
			AssetDatabase.DeleteAsset(kEditorCommunityDLL);			
		}		
	}
}