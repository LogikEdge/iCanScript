using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class iCS_WaitingSequencialDispatcher : iCS_Dispatcher {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_WaitingSequencialDispatcher(string name, int priority) : base(name, priority) {}
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override void Execute(int frameId) {
        bool stalled= true;
        while(myQueueIdx < myExecuteQueue.Count) {
            iCS_Action action= myExecuteQueue[myQueueIdx];
            action.Execute(frameId);            
            if(!action.IsCurrent(frameId)) {
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
        ResetIterator(frameId);
    }
}
