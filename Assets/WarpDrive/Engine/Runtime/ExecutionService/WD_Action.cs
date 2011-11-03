using UnityEngine;
using System.Collections;

/// =========================================================================
// An action is the base class of the execution.  It includes a frame
// identifier that is used to indicate if the action has been run.  This
// indicator is the bases for the execution synchronization.
public abstract class WD_Action : WD_Object {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    protected int  myFrameId= 0;

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public abstract void Execute(int frameId);
    
    // ----------------------------------------------------------------------
    public bool IsCurrent(int frameId)     { return myFrameId == frameId; }
    public void MarkAsCurrent(int frameId) { myFrameId= frameId; }
}
