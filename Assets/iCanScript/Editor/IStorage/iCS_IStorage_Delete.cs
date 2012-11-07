using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class iCS_IStorage {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
	List<iCS_EditorObject>	myDestroyQueue= new List<iCS_EditorObject>();
	
    // ======================================================================
    // ----------------------------------------------------------------------
    public void DestroyInstance(int id) {
		if(!IsValid(id)) return;
        DestroyInstance(EditorObjects[id]);
    }
    // ----------------------------------------------------------------------
    public void DestroyInstance(iCS_EditorObject eObj) {
        DetectUndoRedo();
        DestroyInstanceInternal(eObj);
    }
    // ----------------------------------------------------------------------
    void DestroyInstanceInternal(iCS_EditorObject toDestroy) {
        if(toDestroy == null) return;
		ScheduleDestroyInstance(toDestroy);
		foreach(var obj in myDestroyQueue) {
			DestroySingleObject(obj);
		}
		myDestroyQueue.Clear();
    }
    // ----------------------------------------------------------------------
	void ScheduleDestroyInstance(iCS_EditorObject toDestroy) {
		if(toDestroy == null || toDestroy.InstanceId == -1) return;
		// Don't process if the object has already been processed.
		if(myDestroyQueue.Contains(toDestroy)) return;
		// Schedule all children to be destroyed first.
		toDestroy.ForEachChild(child=> ScheduleDestroyInstance(child));
		// Add the object to the destroy queue.
		myDestroyQueue.Add(toDestroy);
		// Detroy the transition as a single block.
		if(toDestroy.IsStatePort || toDestroy.IsTransitionModule) {
	        iCS_EditorObject outStatePort= GetFromStatePort(toDestroy);
	        iCS_EditorObject inStatePort= GetInTransitionPort(toDestroy);
	        iCS_EditorObject transitionModule= GetTransitionModule(toDestroy);
			if(inStatePort != null)      ScheduleDestroyInstance(inStatePort);
			if(transitionModule != null) ScheduleDestroyInstance(transitionModule);
			if(outStatePort != null)     ScheduleDestroyInstance(outStatePort);
		}
	}
    // ----------------------------------------------------------------------
	void DestroySingleObject(iCS_EditorObject toDestroy) {
		if(toDestroy == null || toDestroy.InstanceId == -1) return;
        // Disconnect ports linking to this port.
        ExecuteIf(toDestroy, obj=> obj.IsPort, _=> DisconnectPort(toDestroy));
        // Update modules runtime data when removing a module port.
        iCS_EditorObject parent= toDestroy.Parent;
        if(toDestroy.IsModulePort || toDestroy.IsInMuxPort) 	 RemoveDynamicPort(toDestroy);
        // Remember entry state.
        bool isEntryState= toDestroy.IsEntryState;
        // Set the parent dirty to force a relayout.
        if(IsValid(toDestroy.ParentId)) SetDirty(parent);
		// Destroy instance.
		toDestroy.DestroyInstance();
        // Reconfigure parent state if the object removed is an entry state.
        if(isEntryState) {
            SelectEntryState(parent);
        }
        myIsDirty= true;
	}
    // ----------------------------------------------------------------------
    void SelectEntryState(iCS_EditorObject parent) {
        bool entryFound= ForEachChild(parent,
            child=> {
                if(child.IsEntryState) {
                    return true;
                }
                return false;
            }
        );        
        if(entryFound) return;
        ForEachChild(parent,
            child=> {
                if(child.IsState) {
                    child.IsRawEntryState= true;
                    return true;
                }
                return false;
            }
        );
    }
}
