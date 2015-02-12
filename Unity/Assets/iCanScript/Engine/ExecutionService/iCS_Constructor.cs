using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

public class iCS_Constructor : iCS_ClassFunction {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Constructor(int instanceId, string name, MethodBase methodBase, iCS_VisualScriptImp visualScript, int priority, int nbOfParameters, int nbOfEnables)
    : base(instanceId, name, methodBase, visualScript, priority, nbOfParameters, nbOfEnables) {}
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoExecute(int runId) {
        if(ReturnValue == null) {
            base.DoExecute(runId);
        } else {
            MarkAsExecuted(runId);
        }
    }
    // ----------------------------------------------------------------------
    protected override void DoForceExecute(int runId) {
        if(ReturnValue == null) {
            base.DoForceExecute(runId);
            if(ReturnValue != null) {
                // TODO: Should remove variable creation for execution queue once done.
                ArePortsAlwaysCurrent= true;
            }
        } else {
            MarkAsExecuted(runId);
        }
    }
    
}
