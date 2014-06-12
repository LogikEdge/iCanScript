using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;

public static class iCS_InstallationController {
    // =================================================================================
    // Installs all needed components
    // ---------------------------------------------------------------------------------
	static iCS_InstallationController() {
        InstallEdition();
        CreateCodeGenerationFolder();		
	}
    public static void Start()    {}
    public static void Shutdown() {}

    
    // =================================================================================
    // Installs the iCanScript Gizmo (if not already done).
    // ---------------------------------------------------------------------------------
	static public void InstallEdition() {
#if TRIAL_EDITION
        // Warning customer of trial period.
        var today= DateTime.Today;
        if(iCS_LicenseController.HasDemoLicense) {
            var lastWarningDate= iCS_PreferencesController.TrialLastWarningDate;
            if(today != lastWarningDate) {
                iCS_PreferencesController.TrialLastWarningDate= today;
                if(iCS_EditionController.IsDemoEdition) {
                    iCS_TrialDialogs.PurchaseDialog();
                }
            }
        }
        else {
            iCS_LicenseController.ResetUserLicense();
            iCS_PreferencesController.TrialStartDate= today;
        }
#else
#if UNITY_STORE_EDITION
        if(iCS_LicenseController.HasDemoLicense) {
            var lastWarningDate= iCS_PreferencesController.TrialLastWarningDate;
            var today= DateTime.Today;
            if(today != lastWarningDate) {
                iCS_PreferencesController.TrialLastWarningDate= today;
                if(iCS_EditionController.IsStoreEdition) {
					var userLicense= iCS_PreferencesController.UserLicense;
					if(String.IsNullOrEmpty(userLicense)) {
						var fingerPrint= iCS_LicenseController.FingerPrint;
						var licenseType= iCS_LicenseType.Pro;
						var version= iCS_Config.MajorVersion;
						var license= iCS_LicenseController.BuildSignature(fingerPrint, (int)licenseType, (int)version);
						iCS_PreferencesController.UserLicense= iCS_LicenseController.ToString(license);						
					}
                }
            }
        }
#else
        if(iCS_LicenseController.HasDemoLicense) {
			if(iCS_EditionController.IsDevEdition) {
				var fingerPrint= iCS_LicenseController.FingerPrint;
				var licenseType= iCS_LicenseType.Pro;
				var version= iCS_Config.MajorVersion;
				var license= iCS_LicenseController.BuildSignature(fingerPrint, (int)licenseType, (int)version);
				iCS_PreferencesController.UserLicense= iCS_LicenseController.ToString(license);
			}                
        }
#endif
#endif
	}

    // ================================================================================
    // Create code generation folder.
    // ---------------------------------------------------------------------------------
    static public void CreateCodeGenerationFolder() {
//        string assetsPath= Application.dataPath;
//        var codeGenerationFolder= iCS_PreferencesEditor.CodeGenerationFolder;
//        string codeGenerationFolderPath= assetsPath+"/"+codeGenerationFolder;
//        if(!Directory.Exists(codeGenerationFolderPath)) {
//            Debug.Log(iCS_Config.ProductName+": Creating Code Generation folder");
//            AssetDatabase.CreateFolder("Assets", codeGenerationFolder);
//        }
//        // Generated behaviour folder.
//        var behavioursSubfolder= iCS_PreferencesEditor.BehaviourGenerationSubfolder;
//        var behavioursPath= codeGenerationFolderPath+"/"+behavioursSubfolder;
//        if(!Directory.Exists(behavioursPath)) {
//            AssetDatabase.CreateFolder("Assets/"+codeGenerationFolder, behavioursSubfolder);            
//        }
    }
    
}
