using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Subspace;

public class iCS_WaitingSequencialDispatcher : iCS_Dispatcher {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_WaitingSequencialDispatcher(string name, SSObject parent, int priority, int nbOfParameters, int nbOfEnables)
    : base(name, parent, priority, nbOfParameters, nbOfEnables) {}
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoEvaluate() {
        bool stalled= true;
        while(myQueueIdx < myExecuteQueue.Count) {
            SSAction action= myExecuteQueue[myQueueIdx];
            action.Evaluate();            
            if(!action.IsEvaluated) {
                // Verify if the child is a staled dispatcher.
                if(!action.IsStalled) {
                    stalled= false;
                }                    
                IsStalled= stalled;
                return;
            }
            stalled= false;
            ++myQueueIdx;
        }
        // Reset iterators for next frame.
        ResetIterator();
    }
}
