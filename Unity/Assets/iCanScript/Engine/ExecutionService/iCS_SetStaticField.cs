using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

public class iCS_SetStaticField : iCS_FunctionBase {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    protected FieldInfo myFieldInfo;

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_SetStaticField(string name, FieldInfo fieldInfo, bool[] paramIsOuts, int priority) : base(name, paramIsOuts, priority) {
        myFieldInfo= fieldInfo;
    }
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoExecute(int frameId) {
        // Execute function
        myFieldInfo.SetValue(null, myParameters[0]);
        MarkAsCurrent(frameId);
    }
}
