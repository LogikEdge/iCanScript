using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using Subspace;

public class iCS_SetInstanceField : iCS_FieldBase {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_SetInstanceField(int instanceId, string name, SSObject parent, FieldInfo fieldInfo, SSContext context, int priority, int nbOfEnables)
    : base(instanceId, name, parent, fieldInfo, context, priority, 1, nbOfEnables) {
    }

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoExecute(int runId) {
        if(!IsThisReady(runId) || !IsParameterReady(0, runId)) {
            IsStalled= true;
            return;
        }
        DoForceExecute(runId);
    }
    // ----------------------------------------------------------------------
    protected override void DoForceExecute(int runId) {
        // Execute function
        UpdateParameter(0);
//#if UNITY_EDITOR
        try {
//#endif
            myFieldInfo.SetValue(This, Parameters[0]);
            MarkAsExecuted(runId);
//#if UNITY_EDITOR
        }
        catch(Exception e) {
            Debug.LogWarning("iCanScript: Exception throw in  "+FullName+" => "+e.Message);
            MarkAsCurrent(runId);
        }
//#endif
    }
}
