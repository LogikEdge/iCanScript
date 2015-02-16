using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Subspace;

public abstract class iCS_Dispatcher : SSActionWithSignature {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    protected List<SSAction> myExecuteQueue= new List<SSAction>();
    protected int              myQueueIdx = 0;
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Dispatcher(string name, SSObject parent, int priority, int nbOfParameters, int nbOfEnables)
    : base(name, parent, priority, nbOfParameters, nbOfEnables) {}

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override Connection GetStalledProducerPort() {
        if(IsCurrent) {
            return null;
        }
        // Get the dispatcher stalled enable ports.
        var producerPort= GetStalledEnablePort(myContext.RunId);
        if(producerPort != null) {
            return producerPort;
        }
        // Get any child stalled ports.
        int cursor= myQueueIdx;
        if(cursor < myExecuteQueue.Count) {
            SSAction action= myExecuteQueue[myQueueIdx];
            if(!action.IsCurrent) {
                producerPort= action.GetStalledProducerPort();
                if(producerPort != null) {
                    return producerPort;
                }
            }
        }
        return null;
    }

    // ----------------------------------------------------------------------
    protected override void DoForceExecute() {
        if(myQueueIdx < myExecuteQueue.Count) {
            SSAction action= myExecuteQueue[myQueueIdx];
            action.ForceExecute();            
            if(action.IsCurrent) {
                ++myQueueIdx;
                IsStalled= false;
            } else {
                IsStalled &= action.IsStalled;
            }
        }
        if(myQueueIdx >= myExecuteQueue.Count) {
            ResetIterator(myContext.RunId);
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
        MarkAsExecuted();        
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
