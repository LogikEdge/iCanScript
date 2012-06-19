using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public static class iCS_EditorMgr {
    // =================================================================================
    // Types
    // ---------------------------------------------------------------------------------
    class EditorInfo {
        public string           Key                   = null;
        public EditorWindow     Window                = null;
        public System.Object    Editor                = null;
        public Action           OnStorageChange       = null;
        public Action           OnSelectedObjectChange= null;  
        public EditorInfo(string key, EditorWindow window, System.Object editor, Action onStorageChange, Action onSelectedObjectChange) {
            Key= key;
            Window= window;
            Editor= editor;
            OnStorageChange= onStorageChange;
            OnSelectedObjectChange= onSelectedObjectChange;
        }
    }
    
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    static List<EditorInfo>   myEditors       = null;
	static int				  myModificationId= 0;
	static bool               myIsPlaying     = false;
    
    // =================================================================================
    // Initialization
    // ---------------------------------------------------------------------------------
    static iCS_EditorMgr() {
        myWindows= new List<EditorInfo>();
    }
    
    // =================================================================================
    // Window management
    // ---------------------------------------------------------------------------------
    public static void Add(string key, EditorWindow window, System.Object editor, Action onStorageChange, Action onSelectedObjectChange) {
        myEditors.Add(new EditorInfo(key, window, editor, onStorageChange, onSelectedObjectChange));
    }
    public static void Remove(string key) {
        int idx= FindIndexOf(key);
        if(idx >= 0) myEditors.RemoveAt(idx);
    }
    
    // =================================================================================
    // Event distribution.
    // ---------------------------------------------------------------------------------
	public static void Update() {
		iCS_StorageMgr.Update();
		bool isPlaying= Application.isPlaying;
		Prelude.filterWith(
			w=> w.IStorage != iCS_StorageMgr.IStorage || myIsPlaying != isPlaying,
			w=> { w.IStorage= iCS_StorageMgr.IStorage; w.OnStorageChange(); w.Editor.Repaint(); },
			myEditors);
		Prelude.filterWith(
			w=> w.SelectedObject != iCS_StorageMgr.SelectedObject || myIsPlaying != isPlaying,
			w=> { w.SelectedObject= iCS_StorageMgr.SelectedObject; w.OnSelectedObjectChange(); w.Editor.Repaint(); },
			myEditors);
		if(iCS_StorageMgr.IStorage != null && myModificationId != iCS_StorageMgr.IStorage.ModificationId) {
			myModificationId= iCS_StorageMgr.IStorage.ModificationId;
			Prelude.forEach(w=> w.Editor.Repaint(), myEditors);
		}
		myIsPlaying= isPlaying;
	}
	
    // ======================================================================
    // Search/Iterations
    // ---------------------------------------------------------------------------------
    static int FindIndexOf(string key) {
        for(int i= 0; i < myEditors.Count; ++i) {
            if(myEditors.Key == key) {
                return i;
            }
        }        
        return -1;
    }
    
    // ======================================================================
    public static EditorWindow FindGraphEditorWindow() {
        int idx= FindIndexOf(typeof(iCS_GraphEditor).Name);
        return idx >= 0 ? myEditors[idx].Window : null;
    } 
    public static EditorWindow FindClassWizardEditorWindow() {
        int idx= FindIndexOf(typeof(iCS_GraphEditor).Name);
        return idx >= 0 ? myEditors[idx].Window : null;
    }
    public static EditorWindow FindHierarchyEditorWindow() {
        int idx= FindIndexOf(typeof(iCS_GraphEditor).Name);
        return idx >= 0 ? myEditors[idx].Window : null;
    }
    public static EditorWindow FindLibraryEditorWindow() {
        int idx= FindIndexOf(typeof(iCS_GraphEditor).Name);
        return idx >= 0 ? myEditors[idx].Window : null;
    }    
    // ======================================================================
    public static iCS_GraphEditor FindGraphEditor() {
        int idx= FindIndexOf(typeof(iCS_GraphEditor).Name);
        return idx >= 0 ? myEditors[idx].Editor as iCS_GraphEditor : null;
    } 
    public static iCS_ClassWizard FindClassWizardEditor() {
        int idx= FindIndexOf(typeof(iCS_ClassWizard).Name);
        return idx >= 0 ? myEditors[idx].Editor as iCS_ClassWizard : null;
    }
    public static iCS_HierarchyEditor FindHierarchyEditor() {
        int idx= FindIndexOf(typeof(iCS_HierarchyEditor).Name);
        return idx >= 0 ? myEditors[idx].Editor as iCS_HierarchyEditor : null;
    }
    public static iCS_LibraryEditor FindLibraryEditor() {
        int idx= FindIndexOf(typeof(iCS_LibraryEditor).Name);
        return idx >= 0 ? myEditors[idx].Editor as iCS_LibraryEditor : null;
    }    
}
