using UnityEngine;
using System.Reflection;
using System.Collections;
using Subspace;

public abstract class iCS_FieldBase : SSActionWithSignature {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    protected FieldInfo myFieldInfo;

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_FieldBase(int instanceId, string name, FieldInfo fieldInfo, SSContext context, int priority,
                         int nbOfParameters, int nbOfEnables)
    : base(instanceId, name, context, priority, nbOfParameters, nbOfEnables) {
        myFieldInfo= fieldInfo;
    }
}
