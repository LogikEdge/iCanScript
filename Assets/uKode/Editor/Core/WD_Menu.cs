using UnityEngine;
using UnityEditor;
using System.Collections;

public class WD_Menu {

    // ======================================================================
	// Create a behavior to selected game object.
	[MenuItem("uKode/Create Behaviour")]
	public static void CreateBehaviour() {
		// Create State Chart component.
		WD_Behaviour storage = Selection.activeGameObject.GetComponent<WD_Behaviour>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent<WD_Behaviour>();
            WD_IStorage iStorage= new WD_IStorage(storage);
            iStorage.CreateBehaviour();
            iStorage= null;
		}
	}
	[MenuItem("uKode/Create Behaviour", true)]
	public static bool ValidateCreateBehaviour() {
		if(Selection.activeTransform != null) {
			WD_Storage storage = Selection.activeGameObject.GetComponent<WD_Storage>();
			return storage == null;
		}
		return false;
	}

    // ======================================================================
	// Create a library module to selected game object.
	[MenuItem("uKode/Create Module Library")]
	public static void CreateModuleLibrary() {
		// Create State Chart component.
		WD_Storage storage = Selection.activeGameObject.GetComponent<WD_ModuleLibrary>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent<WD_ModuleLibrary>();
            storage.enabled= false;
            WD_IStorage iStorage= new WD_IStorage(storage);
            iStorage.CreateModuleLibrary();
            iStorage= null;
		}
	}
	[MenuItem("uKode/Create Module Library", true)]
	public static bool ValidateCreateModuleLibrary() {
		if(Selection.activeTransform != null) {
			WD_Storage storage = Selection.activeGameObject.GetComponent<WD_Storage>();
			return storage == null;
		}
		return false;
	}

    // ======================================================================
	// Create a library module to selected game object.
	[MenuItem("uKode/Create State Chart Library")]
	public static void CreateStateChartLibrary() {
		// Create State Chart component.
		WD_Storage storage = Selection.activeGameObject.GetComponent<WD_StateChartLibrary>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent<WD_StateChartLibrary>();
            storage.enabled= false;
            WD_IStorage iStorage= new WD_IStorage(storage);
            iStorage.CreateStateChartLibrary();
            iStorage= null;
		}
	}
	[MenuItem("uKode/Create State Chart Library", true)]
	public static bool ValidateCreateStateChartLibrary() {
		if(Selection.activeTransform != null) {
			WD_Storage storage = Selection.activeGameObject.GetComponent<WD_Storage>();
			return storage == null;
		}
		return false;
	}
    
    // ----------------------------------------------------------------------
    bool IsStorageAlreadyPresent() {
        return false;
    }
    
    // ======================================================================
	// uKode Window.
	[MenuItem("Window/uKode Editor")]
	public static void ShowuKodeEditor() {
        WD_Editor editor= EditorWindow.GetWindow<WD_Editor>();
        editor.hideFlags= HideFlags.DontSave;
	}

}
