using UnityEngine;
using System.Collections;

public static class iCS_EditorStartup {
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
	static iCS_EditorStartup() {
		iCS_SoftwareUpdateController.PeriodicUpdateVerification();
		iCS_SystemEvents.Start();
		iCS_InstallerMgr.Start();
		iCS_CodeGenerator.Start();		
	}
	public static void Start() {}
}
