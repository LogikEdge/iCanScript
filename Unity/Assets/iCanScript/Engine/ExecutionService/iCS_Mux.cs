using UnityEngine;
using System.Collections;

public class iCS_Mux : iCS_ActionWithSignature {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Mux(iCS_VisualScriptImp visualScript, int priority, int nbOfParameters)
    : base(visualScript, priority, nbOfParameters, 0) {}

    // ======================================================================
    // Execution (not used)
    // ----------------------------------------------------------------------
    // FIXME: Mux should prefer running over current frame nodes.
    protected override void DoExecute(int runId) {
        // Take the first valid connection.
        foreach(var connection in ParameterConnections) {
            if(connection.DidExecute(runId)) {
                ReturnValue= connection.Value;
                MarkAsExecuted(runId);
                return;
            }
        }
    }
    // ----------------------------------------------------------------------
    protected override void DoForceExecute(int runId) {
        // Take the last that has executed.
		int smallestDistance= 100000;
		iCS_Connection bestConnection= null;
        foreach(var connection in ParameterConnections) {
			if(connection == null) continue;
			var action= connection.Action;
			if(action == null) continue;
			int runIdDistance= runId-action.ExecutionRunId;
			if(runIdDistance < smallestDistance) {
				smallestDistance= runIdDistance;
				bestConnection= connection;
			}
        }
		// Take value from the last that executed.
		if(bestConnection != null) {
            ReturnValue= bestConnection.Value;			
		}
        MarkAsCurrent(runId);
    }
}
