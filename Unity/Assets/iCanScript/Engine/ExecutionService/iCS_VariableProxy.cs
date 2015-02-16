using UnityEngine;
using System.Collections;
using Subspace;

public class iCS_VariableProxy : SSActionWithSignature {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_VariableProxy(string name, SSObject parent, int priority,
                             int nbOfParameters, int nbOfEnables)
    : base(name, parent, priority, nbOfParameters, nbOfEnables) {
    }

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoEvaluate() {
        MarkAsExecuted();            
    }

    // ----------------------------------------------------------------------
    protected override void DoExecute() {
        MarkAsExecuted();            
    }
}
