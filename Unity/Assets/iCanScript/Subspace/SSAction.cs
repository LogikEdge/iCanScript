using UnityEngine;
using System.Collections;

namespace Subspace {
    
    /// =========================================================================
    // An action is the base class of the execution.  It includes a frame
    // identifier that is used to indicate if the action has been run.  This
    // indicator is the bases for the execution synchronization.
    public abstract class SSAction : SSObject {
        // ======================================================================
        // Properties
        // ----------------------------------------------------------------------
        int         myCurrentRunId  = 0;
        int         myExecutedRunId= 0;
        bool        myIsStalled= false;
        bool        myIsActive= true;
        bool        myPortsAreAlwaysCurrent= false;
		protected SSRunState myRunState= SSRunState.WAITING;

        // ======================================================================
        // Accessors
        // ----------------------------------------------------------------------
        public int      RunId               { get { return CurrentRunId; }}
        public int      CurrentRunId        { get { return myCurrentRunId; }}
        public int      ExecutionRunId      { get { return myExecutedRunId; }}
        public bool     IsStalled           { get { return myIsStalled; } set { myIsStalled= value; }}
        public SSAction ParentAction        { get { return myParent as SSAction; } set { myParent= value; }}
        public bool IsActive            {
            get {
                if(myIsActive == false) {
                    return false;
                }
                var pAction= ParentAction;
                return pAction == null ? true : pAction.IsActive;
            }
            set { myIsActive= value; }
        }
        public bool ArePortsAlwaysCurrent { get { return myPortsAreAlwaysCurrent; } set { myPortsAreAlwaysCurrent= value; }}
    
        // ======================================================================
        // Creation/Destruction
        // ----------------------------------------------------------------------
        public SSAction(iCS_VisualScriptImp visualScript, int priority)
        : base(visualScript, priority) {}
     
        // ======================================================================
        // Execution
        // ----------------------------------------------------------------------
        public abstract void            Execute(int runId);
        public abstract void            ForceExecute(int runId);
        public abstract Connection      GetStalledProducerPort(int runId);
    
        // ----------------------------------------------------------------------
        public bool IsCurrent(int runId)      { return myCurrentRunId == runId; }
        public bool DidExecute(int runId)     { return myExecutedRunId == runId; }
        public void MarkAsCurrent(int runId)  { myCurrentRunId= runId; myIsStalled= false; }
        public void MarkAsExecuted(int runId) { myExecutedRunId= runId; MarkAsCurrent(runId); }

        // ----------------------------------------------------------------------
        public bool ArePortsCurrent(int runId)    { return IsCurrent(runId) || ArePortsAlwaysCurrent || !IsActive; }
        public bool ArePortsExecuted(int runId)   { return DidExecute(runId); }
    }
    
}
