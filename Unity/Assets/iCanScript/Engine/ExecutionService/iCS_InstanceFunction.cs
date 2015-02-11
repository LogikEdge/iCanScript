using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

public class iCS_InstanceFunction : iCS_ClassFunction {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_InstanceFunction(MethodBase methodBase, iCS_VisualScriptImp visualScript, int priority, int nbOfParameters, int nbOfEnables)
    : base(methodBase, visualScript, priority, nbOfParameters, nbOfEnables) {
    }
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoExecute(int runId) {
        if(IsThisReady(runId)) {
            base.DoExecute(runId);
        }
    }
}
