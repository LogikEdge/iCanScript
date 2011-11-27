using UnityEngine;
using System;
using System.Reflection;
using System.Collections;

public class UK_SetStaticField : UK_FunctionBase {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    protected FieldInfo myFieldInfo;

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public UK_SetStaticField(string name, FieldInfo fieldInfo, object[] parameters, bool[] paramIsOuts) : base(name, parameters, paramIsOuts) {
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
