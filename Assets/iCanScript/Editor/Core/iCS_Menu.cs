using UnityEngine;
using UnityEditor;
using System.Collections;

public class iCS_Menu {

    // ======================================================================
	// Create a behavior to selected game object.
	//[MenuItem("iCanScript/Create Behaviour #&b", false, 1)]
	public static void CreateBehaviour() {
		// Create State Chart component.
		iCS_Behaviour storage = Selection.activeGameObject.GetComponent<iCS_Behaviour>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent<iCS_Behaviour>();
            iCS_IStorage iStorage= new iCS_IStorage(storage);
            iStorage.CreateBehaviour();
            iStorage= null;
		}
	}
	//[MenuItem("iCanScript/Create Behaviour #&b", true, 1)]
	public static bool ValidateCreateBehaviour() {
        // Validate add behaviour
		if(Selection.activeTransform != null) {
			iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Storage>();
			return storage == null;
		}
		return false;
	}

    // ======================================================================
	// Create a library module to selected game object.
	//[MenuItem("iCanScript/Create Module Library", false, 2)]
	public static void CreateModuleLibrary() {
		// Create State Chart component.
		iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Library>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent<iCS_Library>();
            storage.enabled= false;
            iCS_IStorage iStorage= new iCS_IStorage(storage);
            iStorage.CreateModule(-1, Vector2.zero, "Module Library");
            iStorage= null;
		}
	}
	//[MenuItem("iCanScript/Create Module Library", true, 2)]
	public static bool ValidateCreateModuleLibrary() {
		if(Selection.activeTransform != null) {
			iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Storage>();
			return storage == null;
		}
		return false;
	}

    // ======================================================================
	// Create a library module to selected game object.
	//[MenuItem("iCanScript/Create State Chart Library", false, 3)]
	public static void CreateStateChartLibrary() {
		// Create State Chart component.
		iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Library>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent<iCS_Library>();
            storage.enabled= false;
            iCS_IStorage iStorage= new iCS_IStorage(storage);
            iStorage.CreateStateChart(-1, Vector2.zero, "State Chart Library");
            iStorage= null;
		}
	}
	//[MenuItem("iCanScript/Create State Chart Library", true, 3)]
	public static bool ValidateCreateStateChartLibrary() {
		if(Selection.activeTransform != null) {
			iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Storage>();
			return storage == null;
		}
		return false;
	}
    
    // ----------------------------------------------------------------------
    bool IsStorageAlreadyPresent() {
        return false;
    }
    
//    // ======================================================================
//	// iCanScript License.
//    [MenuItem("iCanScript/Get FingerPrint")]
//    public static void GetFingerPrint() {
//        Debug.Log(iCS_LicenseUtil.ToString(iCS_FingerPrint.FingerPrint));
//    }
//    [MenuItem("iCanScript/Get Standard License")]
//    public static void GetStandardLicense() {
//        Debug.Log(iCS_LicenseUtil.ToString(iCS_UnlockKeyGenerator.Standard));
//    }
//    [MenuItem("iCanScript/Get Pro License")]
//    public static void GetProLicense() {
//        Debug.Log(iCS_LicenseUtil.ToString(iCS_UnlockKeyGenerator.Pro));
//    }
//    [MenuItem("iCanScript/License Manager")]
//    public static void EnterLicense() {
//    }
    
    // ======================================================================
    // Navigation
    //[MenuItem("iCanScript/",false,20)]
    //[MenuItem("iCanScript/Center Graph _#&f",false,21)]
    public static void CenterGraph() {
//        iCS_EditorProxy editorProxy= EditorWindow.GetWindow(typeof(iCS_EditorProxy), false, "iCanScript") as iCS_EditorProxy;
//        editorProxy.Editor.CenterOnRoot();
    }
    //[MenuItem("iCanScript/Center On Selected _&f",false,22)]
    public static void CenterOnSelected() {
//        iCS_EditorProxy editorProxy= EditorWindow.GetWindow(typeof(iCS_EditorProxy), false, "iCanScript") as iCS_EditorProxy;
//        editorProxy.Editor.CenterOnSelected();
    }
    // ======================================================================
    // Documentation Access
    //[MenuItem("iCanScript/",false,30)]
    //[MenuItem("iCanScript/Documentation/Home Page",false,31)]
    public static void HomePage() {
        Application.OpenURL("http://www.icanscript.com/index.html");
    }
    //[MenuItem("iCanScript/Documentation/User's Manual",false,32)]
    public static void UserManual() {
        Application.OpenURL("http://www.icanscript.com/Documentation/UserManual/index.html");
    }
    //[MenuItem("iCanScript/Documentation/Programmer's Guide",false,33)]
    public static void ProgrammerGuide() {
        Application.OpenURL("http://www.icanscript.com/Documentation/ProgrammerGuide/index.html");
    }
    //[MenuItem("iCanScript/Documentation/Release Notes",false,34)]
    public static void ReleaseNotes() {
        Application.OpenURL("http://www.icanscript.com/Documentation/ReleaseNotes/index.html");
    }
    // ======================================================================
    // Support Access
    //[MenuItem("iCanScript/Report a Bug",false,40)]
    public static void ReportBug() {
        Application.OpenURL("http://www.icanscript.com/Support/ReportBug/index.html");
    }
}
