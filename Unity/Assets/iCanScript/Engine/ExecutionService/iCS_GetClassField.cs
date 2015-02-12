using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

public class iCS_GetClassField : iCS_FieldBase {
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_GetClassField(int instanceId, string name, FieldInfo fieldInfo, iCS_VisualScriptImp visualScript, int priority, int nbOfEnables)
    : base(instanceId, name, fieldInfo, visualScript, priority, 0, nbOfEnables) {}
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoExecute(int runId) {
        DoForceExecute(runId);
    }
    // ----------------------------------------------------------------------
    protected override void DoForceExecute(int runId) {
//#if UNITY_EDITOR
        try {
//#endif
            // Execute function
            ReturnValue= myFieldInfo.GetValue(This);
            MarkAsExecuted(runId);
//#if UNITY_EDITOR
        }
        catch(Exception e) {
            Debug.LogWarning("iCanScript: Exception thrown in  "+FullName+" => "+e.Message);
            MarkAsCurrent(runId);
        }
//#endif        
    }
}
