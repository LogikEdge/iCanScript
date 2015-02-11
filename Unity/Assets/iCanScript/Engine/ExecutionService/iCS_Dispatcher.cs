using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Subspace;

public abstract class iCS_Dispatcher : iCS_ActionWithSignature {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    protected List<SSAction> myExecuteQueue= new List<SSAction>();
    protected int              myQueueIdx = 0;
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Dispatcher(iCS_VisualScriptImp visualScript, int priority, int nbOfParameters, int nbOfEnables)
    : base(visualScript, priority, nbOfParameters, nbOfEnables) {}

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override iCS_Connection GetStalledProducerPort(int runId) {
        if(IsCurrent(runId)) {
            return null;
        }
        var producerPort= mySignature.GetStalledProducerPort(runId, /*enablesOnly=*/true);
        if(producerPort != null) {
            return producerPort;
        }
        int cursor= myQueueIdx;
        if(cursor < myExecuteQueue.Count) {
            SSAction action= myExecuteQueue[myQueueIdx];
            if(!action.IsCurrent(runId)) {
                producerPort= action.GetStalledProducerPort(runId);
                if(producerPort != null) {
                    return producerPort;
                }
            }
        }
        return null;
    }

    // ----------------------------------------------------------------------
    protected override void DoForceExecute(int runId) {
        if(myQueueIdx < myExecuteQueue.Count) {
            SSAction action= myExecuteQueue[myQueueIdx];
            action.ForceExecute(runId);            
            if(action.IsCurrent(runId)) {
                ++myQueueIdx;
                IsStalled= false;
            } else {
                IsStalled &= action.IsStalled;
            }
        }
        if(myQueueIdx >= myExecuteQueue.Count) {
            ResetIterator(runId);
        }
    }
    // ----------------------------------------------------------------------
    protected void Swap(int idx1, int idx2) {
        var tmp= myExecuteQueue[idx1];
        myExecuteQueue[idx1]= myExecuteQueue[idx2];
        myExecuteQueue[idx2]= tmp;
    }
    // ----------------------------------------------------------------------
    protected void ResetIterator(int runId) {
        myQueueIdx= 0;
        MarkAsExecuted(runId);        
    }
    
    // ======================================================================
    // Queue Management
    // ----------------------------------------------------------------------
    public void AddChild(SSAction action) {
        myExecuteQueue.Add(action);
        action.ParentAction= this;
    }
    public void RemoveChild(SSAction action) {
        myExecuteQueue.Remove(action);
        action.ParentAction= null;
    }
}
