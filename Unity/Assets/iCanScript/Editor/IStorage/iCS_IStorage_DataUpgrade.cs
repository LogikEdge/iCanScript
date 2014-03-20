using UnityEngine;
using UnityEditor;
using System.Collections;

public partial class iCS_IStorage {
    // ----------------------------------------------------------------------
	void PerformEngineDataUpgrade() {
		iCS_Version storageVersion= new iCS_Version(MonoBehaviourStorage.MajorVersion, MonoBehaviourStorage.MinorVersion, MonoBehaviourStorage.BugFixVersion);
		iCS_Version softwareVersion= new iCS_Version(iCS_Config.MajorVersion, iCS_Config.MinorVersion, iCS_Config.BugFixVersion);
		if(softwareVersion.IsEqual(storageVersion)) { return; }
		
		// v0.9.3: Need to convert behaviour module to message 
		if(!storageVersion.IsOlderThen(0,9,3)) {
			ShowUpgradeDialog(softwareVersion);
			v0_9_3_Upgrade();
			SaveCurrentScene();
		}
		// Update storage version identifiers
		MonoBehaviourStorage.MajorVersion = iCS_Config.MajorVersion;
		MonoBehaviourStorage.MinorVersion = iCS_Config.MinorVersion;
		MonoBehaviourStorage.BugFixVersion= iCS_Config.BugFixVersion;
	}

    // ----------------------------------------------------------------------
	// Convert module under behaviour to message.
	void v0_9_3_Upgrade() {
        // Convert behaviour type
        if(MonoBehaviourStorage.EngineObjects.Count != 0) {
            MonoBehaviourStorage.EngineObjects[0].QualifiedType= typeof(iCS_VisualScriptImp).AssemblyQualifiedName;
        }
		foreach(var n in MonoBehaviourStorage.EngineObjects) {
			if(n.IsValid && n.IsParentValid) {
				// Convert behaviour modules to messages.
				var parent= MonoBehaviourStorage.EngineObjects[n.ParentId];
				if(parent.IsBehaviour) {
					if(n.IsKindOfPackage) {
						n.ObjectType= iCS_ObjectTypeEnum.InstanceMessage;						
					}
				}
				// Convert StaticModulePort to FixPort.
				if(n.ObjectType == iCS_ObjectTypeEnum.InStaticModulePort_obsolete) {
					n.ObjectType= iCS_ObjectTypeEnum.InFixDataPort;
				}
				if(n.ObjectType == iCS_ObjectTypeEnum.OutStaticModulePort_obsolete) {
					n.ObjectType= iCS_ObjectTypeEnum.OutFixDataPort;
				}
			}
		}
	}
	
    // ----------------------------------------------------------------------
	void ShowUpgradeDialog(iCS_Version softwareVersion) {
		EditorUtility.DisplayDialog("iCanScript Data Upgrade Required", "Your visual scripts were created with an earlier version of iCanScript.\n\nAn upgrade to v"+softwareVersion.ToString()+" will be performed in memory.\nPlease save your scenes to complete the upgrade.", "Ok");
	}
    // ----------------------------------------------------------------------
	void SaveCurrentScene() {
		EditorApplication.SaveCurrentSceneIfUserWantsTo();				
	}
	
}
