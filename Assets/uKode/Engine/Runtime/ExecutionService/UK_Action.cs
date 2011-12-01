using UnityEngine;
using System.Collections;

/// =========================================================================
// An action is the base class of the execution.  It includes a frame
// identifier that is used to indicate if the action has been run.  This
// indicator is the bases for the execution synchronization.
public abstract class UK_Action : UK_Object {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    int  myFrameId= 0;

    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public int FrameId { get { return myFrameId; }}
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public UK_Action(string name) : base(name) {}
     
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public abstract void Execute(int frameId);
    public virtual  void ForceExecute(int frameId) { Execute(frameId); }
    
    // ----------------------------------------------------------------------
    public bool IsCurrent(int frameId)     { return myFrameId == frameId; }
    public void MarkAsCurrent(int frameId) { myFrameId= frameId; }
}
