using UnityEngine;
using UnityEditor;
using P=Prelude;

public static class iCS_iCanScriptMenu {
    // ======================================================================
	// Create a behavior to selected game object.
	[MenuItem("iCanScript/Create Visual Script", false, 1)]
	public static void CreateVisualScript() {
		iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Storage>();
		if(storage == null) {
			storage= Selection.activeGameObject.AddComponent("iCS_VisualScript") as iCS_VisualScriptImp;
            iCS_IStorage iStorage= new iCS_IStorage(storage);
            iStorage.CreateBehaviour();
            iStorage= null;
		}
	}
	[MenuItem("iCanScript/Create Visual Script", true, 1)]
	public static bool ValidateCreateVisualScript() {
		if(Selection.activeTransform == null) return false;
		iCS_Storage storage = Selection.activeGameObject.GetComponent<iCS_Storage>();
		return storage == null;
	}

    // ======================================================================
    // Navigation
    [MenuItem("iCanScript/",false,20)]
    [MenuItem("iCanScript/Center Visual Script #f",false,21)]
    public static void FocusOnVisualScript() {
        iCS_Menu.FocusOnVisualScript();
    }
    [MenuItem("iCanScript/Focus On Selected _f",false,22)]
    public static void FocusOnSelected() {
        iCS_Menu.FocusOnSelected();
    }
    // ======================================================================
    // Documentation Access
    [MenuItem("iCanScript/",false,30)]
    [MenuItem("iCanScript/Documentation/Home Page",false,31)]
    public static void HomePage() {
        Application.OpenURL("http://www.icanscript.com");
    }
    [MenuItem("iCanScript/Documentation/User's Manual",false,32)]
    public static void UserManual() {
        Application.OpenURL("http://www.icanscript.com/documentation/user_guide");
    }
    [MenuItem("iCanScript/Documentation/Release Notes",false,34)]
    public static void ReleaseNotes() {
        Application.OpenURL("http://www.icanscript.com/support/release_notes");
    }
    // ======================================================================
    // Support Access
    [MenuItem("iCanScript/Customer Request",false,60)]
    public static void ReportBug() {
        Application.OpenURL("http://www.disruptive-sw.com/support/customer_request");
    }
    [MenuItem("iCanScript/Check for Updates...",false,61)]
    public static void CheckForUpdate() {
		var isLatest= iCS_InstallerMgr.CheckForUpdates();
		if(isLatest.isNothing) {
			EditorUtility.DisplayDialog("Unable to determine latest version !!!",
										"Problem accessing iCanScript version information.\nPlease try again later.",
										"Ok");			
			return;
		}
        if(isLatest.Value) {
			EditorUtility.DisplayDialog("You have the latest version of iCanScript!",
										 "The version installed is: v"+iCS_EditorConfig.VersionId+".\nNo updates are available.",
										 "Ok");
		}
    }
}
