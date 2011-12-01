using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UK_ParallelDispatcher : UK_DispatcherBase {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    const int retriesBeforeDeclaringStaled= 3;
          int myNbOfRetries= 0;
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public UK_ParallelDispatcher(string name) : base(name) {}
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override void Execute(int frameId) {
        bool stalled= true;
        int tries= 0;
        int maxTries= myExecuteQueue.Count-myQueueIdx;
        while(myQueueIdx < myExecuteQueue.Count) {
            // Attempt to execute child function.
            UK_Action action= myExecuteQueue[myQueueIdx];
            action.Execute(frameId);            
            // Move to next child if sucessfully executed.
            if(action.IsCurrent(frameId)) {
                stalled= false;
                ++myQueueIdx;
                continue;
            }
            // Verify if the child is a staled dispatcher.
            UK_IDispatcher childDispatcher= action as UK_IDispatcher;
            if(childDispatcher != null && !childDispatcher.IsStalled) {
                stalled= false;
            }
            // Return if we have seen too many staled children.
            if(++tries > maxTries) {
                if(!stalled) {
                    myNbOfRetries= 0;
                    myIsStalled= false;
                } else {
                    if(++myNbOfRetries > retriesBeforeDeclaringStaled) {
                        myIsStalled= true;
                    }                    
                }
                return;
            }
            // The function is not ready to execute so lets delay the execution.
            myExecuteQueue.RemoveAt(myQueueIdx);
            myExecuteQueue.Add(action);
        }
        // Reset iterators for next frame.
        myIsStalled= false;
        myQueueIdx= 0;
        myNbOfRetries= 0;
        MarkAsCurrent(frameId);
    }
}
