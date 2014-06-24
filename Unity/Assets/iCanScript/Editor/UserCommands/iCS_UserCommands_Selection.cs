﻿
// File: iCS_UserCommands_Selection
//
using UnityEngine;
using UnityEditor;
using System.Collections;
using P=Prelude;

public static partial class iCS_UserCommands {
    // ----------------------------------------------------------------------
	public static void Select(iCS_EditorObject obj, iCS_IStorage iStorage) {
        if(obj == iStorage.SelectedObject) return;
		iStorage.SelectedObject= obj;
        if(obj != null && obj.IsNode && obj.IsParentOf(iStorage.DisplayRoot)) {
            iCS_UserCommands.SetAsDisplayRoot(obj);
        }
        iCS_EditorController.RepaintInstanceEditor();
	}
    // ----------------------------------------------------------------------
	public static void ToggleMultiSelection(iCS_EditorObject obj) {
        if(obj == null) return;
        var iStorage= obj.IStorage;
		iStorage.ToggleMultiSelection(obj);
	}

}
