using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public static class iCS_EditorMgr {
    // =================================================================================
    // Fields
    // ---------------------------------------------------------------------------------
    static List<iCS_EditorBase>   myEditors= null;
    
    // =================================================================================
    // Initialization
    // ---------------------------------------------------------------------------------
    static iCS_EditorMgr() {
        myEditors= new List<iCS_EditorBase>();
    }
    
    // =================================================================================
    // Window management
    // ---------------------------------------------------------------------------------
    // FIXME: The relationship between proxy type and editor name needs to be cleaned up.
    public static void Add(iCS_EditorBase editor) {
        myEditors.Add(editor);
    }
    public static void Remove(iCS_EditorBase editor) {
        int idx= FindIndexOf(editor);
        if(idx >= 0) myEditors.RemoveAt(idx);
    }
    
    // =================================================================================
    // Event distribution.
    // ---------------------------------------------------------------------------------
	public static void Update() {
        // Update storage information for selected object.
		iCS_StorageMgr.Update();
        foreach(var editor in myEditors) {
            editor.Repaint();
        }
	}
	
    // =================================================================================
    // Search/Iterations
    // ---------------------------------------------------------------------------------
    static int FindIndexOf(iCS_EditorBase editor) {
        for(int i= 0; i < myEditors.Count; ++i) {
            if(myEditors[i] == editor) {
                return i;
            }
        }        
        return -1;
    }
    
    // ======================================================================
    public static void ShowVisualEditor() {
        EditorApplication.ExecuteMenuItem("Window/iCanScript/Visual Editor");
    }
    public static void ShowInstanceEditor() {
        EditorApplication.ExecuteMenuItem("Window/iCanScript/Instance Wizard");        
    }
    public static void ShowHierarchyEditor() {
        EditorApplication.ExecuteMenuItem("Window/iCanScript/Hierarchy");                
    }
    public static void ShowLibraryEditor() {
        EditorApplication.ExecuteMenuItem("Window/iCanScript/Library");                
    }
    public static void ShowPreferences() {
        EditorApplication.ExecuteMenuItem("Window/iCanScript/Preferences");                
    }

    // ======================================================================
    public static EditorWindow FindWindow(Type type) {
        foreach(var ed in myEditors) {
            if(iCS_Types.IsA(type, ed.GetType())) {
                return ed;
            }
        }
        return null;
    }
    public static EditorWindow FindWindow<T>() {
        return FindWindow(typeof(T));
    }
    public static EditorWindow FindVisualEditorWindow() {
        return FindWindow<iCS_VisualEditor>();
    } 
    public static EditorWindow FindInstanceEditorWindow() {
        return FindWindow<iCS_InstanceEditor>();
    }
    public static EditorWindow FindTreeViewEditorWindow() {
        return FindWindow<iCS_TreeViewEditor>();
    }
    public static EditorWindow FindLibraryEditorWindow() {
        return FindWindow<iCS_LibraryEditor>();
    }    
//    public static EditorWindow FindInspectorWindow() {
//        
//    }
    // ======================================================================
    public static iCS_VisualEditor FindVisualEditor() {
        return FindWindow(typeof(iCS_VisualEditor)) as iCS_VisualEditor;
    } 
    public static iCS_InstanceEditor FindInstanceEditor() {
        return FindWindow(typeof(iCS_InstanceEditor)) as iCS_InstanceEditor;
    }
    public static iCS_TreeViewEditor FindTreeViewEditor() {
        return FindWindow(typeof(iCS_TreeViewEditor)) as iCS_TreeViewEditor;
    }
    public static iCS_LibraryEditor FindLibraryEditor() {
        return FindWindow(typeof(iCS_LibraryEditor)) as iCS_LibraryEditor;
    }    
    public static iCS_PreferencesEditor FindPreferencesEditor() {
        return FindWindow(typeof(iCS_PreferencesEditor)) as iCS_PreferencesEditor;
    }
    // ======================================================================
	public static void RepaintVisualEditor() {
		var editor= FindVisualEditor();
		if(editor != null) {
			editor.Repaint();			
		}
	}
	public static void RepaintPreferencesEditor() {
		var editor= FindPreferencesEditor();
		if(editor != null) {
			editor.Repaint();			
		}
	}
//    public static void RepaintInspector() {
//        var editor= FindInspector();
//        if(editor != null) {
//            editor.Repaint();
//        }
//    }
}
