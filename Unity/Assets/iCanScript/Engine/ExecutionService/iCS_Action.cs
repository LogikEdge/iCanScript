using UnityEngine;
using System.Collections;

/// =========================================================================
// An action is the base class of the execution.  It includes a frame
// identifier that is used to indicate if the action has been run.  This
// indicator is the bases for the execution synchronization.
public abstract class iCS_Action : iCS_Object {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    int  myCurrentFrameId  = 0;
    int  myExecutedFrameId= 0;
    bool myIsStalled= false;
    bool myIsEnabled= true;

    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public int  FrameId             { get { return CurrentFrameId; }}
    public int  CurrentFrameId      { get { return myCurrentFrameId; }}
    public int  ExecutionFrameId    { get { return myExecutedFrameId; }}
    public bool IsStalled           { get { return myIsStalled; } set { myIsStalled= value; }}
    public bool IsEnabled           { get { return myIsEnabled; } set { myIsEnabled= value; }}
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Action(iCS_VisualScriptImp visualScript, int priority)
    : base(visualScript, priority) {}
     
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public abstract void Execute(int frameId);
    public abstract void ForceExecute(int frameId);
    public abstract iCS_Connection  GetStalledProducerPort(int frameId);
    
    // ----------------------------------------------------------------------
    public bool IsCurrent(int frameId)      { return myCurrentFrameId == frameId; }
    public bool DidExecute(int frameId)     { return myExecutedFrameId == frameId; }
    public void MarkAsCurrent(int frameId)  { myCurrentFrameId= frameId; myIsStalled= false; }
    public void MarkAsExecuted(int frameId) { myExecutedFrameId= frameId; MarkAsCurrent(frameId); }

    // ----------------------------------------------------------------------
    public bool ArePortsCurrent(int frameId)    { return IsCurrent(frameId) || !IsEnabled; }
    public bool ArePortsExecuted(int frameId)   { return DidExecute(frameId) || !IsEnabled; }
}
