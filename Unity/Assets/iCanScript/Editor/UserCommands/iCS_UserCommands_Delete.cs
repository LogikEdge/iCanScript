//
// File: iCS_UserCommands_Delete
//
//#define DEBUG
using UnityEngine;
using System.Collections;
using P=Prelude;
using iCanScriptEditor;

public static partial class iCS_UserCommands {
    // ======================================================================
    // Object destruction.
	// ----------------------------------------------------------------------
    public static void DeleteObject(iCS_EditorObject obj) {
#if DEBUG
		Debug.Log("iCanScript: Deleting => "+obj.Name);
#endif
        if(obj == null || obj == obj.IStorage.DisplayRoot) return;
        if(!IsDeletionAllowed()) return;
        var name= obj.Name;
        if(!obj.CanBeDeleted()) {
            ShowNotification("Fix port=> \""+name+"\" from node=> \""+obj.ParentNode.FullName+"\" cannot be deleted.");
            return;
        }
        var iStorage= obj.IStorage;
        OpenTransaction(iStorage);
        if(obj.IsInstanceNodePort) {
            try {
        		iStorage.AnimateGraph(null,
                    _=> {
                        iStorage.SelectedObject= obj.ParentNode;
						SystemEvents.AnnounceVisualScriptElementWillBeRemoved(obj);
                        iStorage.InstanceWizardDestroyAllObjectsAssociatedWithPort(obj);
                        iStorage.ForcedRelayoutOfTree();
                    }
        		);                
            }
            catch(System.Exception) {
                CancelTransaction(iStorage);
                return;
            }
            iCS_EditorController.RepaintInstanceEditor();
            CloseTransaction(iStorage, "Delete "+name);
            return;
        }
        // TODO: Should animate parent node on node delete.
        try {
    		iStorage.AnimateGraph(null,
                _=> {
                    // Move the selection to the parent node
                    var parent= obj.ParentNode;
                    iStorage.SelectedObject= parent;
					SystemEvents.AnnounceVisualScriptElementWillBeRemoved(obj);
                    iStorage.DestroyInstance(obj.InstanceId);
                    iStorage.ForcedRelayoutOfTree();
                }
    		);            
        }
        catch(System.Exception) {
            CancelTransaction(iStorage);
            return;
        }
        CloseTransaction(iStorage, "Delete "+name);
	}
	// ----------------------------------------------------------------------
    public static bool DeleteMultiSelectedObjects(iCS_IStorage iStorage) {
#if DEBUG
		Debug.Log("iCanScript: Multi-Select Delete");
#endif
        if(iStorage == null) return false;
        if(!IsDeletionAllowed()) return false;
        var selectedObjects= iStorage.GetMultiSelectedObjects();
        if(selectedObjects == null || selectedObjects.Length == 0) return false;
        if(selectedObjects.Length == 1) {
            DeleteObject(selectedObjects[0]);
            return true;
        }
        OpenTransaction(iStorage);
        try {
            iStorage.AnimateGraph(null,
                _=> {
                    foreach(var obj in selectedObjects) {
                        if(!obj.CanBeDeleted()) {
                            ShowNotification("Fix port=> \""+obj.Name+"\" from node=> \""+obj.ParentNode.FullName+"\" cannot be deleted.");
                            continue;
                        }
                        // Move the selection to the parent node
                        var parent= obj.ParentNode;
                        iStorage.SelectedObject= parent;

						SystemEvents.AnnounceVisualScriptElementWillBeRemoved(obj);

                        if(obj.IsInstanceNodePort) {
                    		iStorage.InstanceWizardDestroyAllObjectsAssociatedWithPort(obj);                        
                        }
                        else {
                    		iStorage.DestroyInstance(obj.InstanceId);                        
                        }
                        iStorage.ForcedRelayoutOfTree();
                    }                
                }
            );            
        }
        catch(System.Exception) {
            CancelTransaction(iStorage);
            return false;
        }
        CloseTransaction(iStorage, "Delete Selection");
        return true;
    }
	// ----------------------------------------------------------------------
    public static void DeleteKeepChildren(iCS_EditorObject obj) {
        if(!IsDeletionAllowed()) return;
        var iStorage= obj.IStorage;
        OpenTransaction(iStorage);
        try {
            var newParent= obj.ParentNode;
            var childNodes= obj.BuildListOfChildNodes(_ => true);
            var childPos= P.map(n => n.GlobalPosition, childNodes);
            iStorage.AnimateGraph(obj,
                _=> {
                    // Move the selection to the parent node
                    var parent= obj.ParentNode;
                    iStorage.SelectedObject= parent;
                
                    P.forEach(n => { iStorage.ChangeParent(n, newParent);}, childNodes);
					SystemEvents.AnnounceVisualScriptElementWillBeRemoved(obj);
                    iStorage.DestroyInstance(obj.InstanceId);
                    P.zipWith((n,p) => { n.LocalAnchorFromGlobalPosition= p; }, childNodes, childPos);
                    iStorage.ForcedRelayoutOfTree();
                }
            );            
        }
        catch(System.Exception) {
            CancelTransaction(iStorage);
            return;
        }
        CloseTransaction(iStorage, "Delete "+obj.Name);
    }
    // ----------------------------------------------------------------------
    public static void RemoveUnusedPorts(iCS_EditorObject messageHandler) {
        if(messageHandler == null) return;
        var iStorage= messageHandler.IStorage;
        OpenTransaction(iStorage);
		try {
			iStorage.AnimateGraph(null,
				_=> {
					iStorage.RemoveUnusedPorts(messageHandler);
					iStorage.ForcedRelayoutOfTree();
				}
			);
		}
        catch(System.Exception) {
            CancelTransaction(iStorage);
            return;
        }
        CloseTransaction(iStorage, "Remove Unused Ports=> "+messageHandler.Name);
	}

}
