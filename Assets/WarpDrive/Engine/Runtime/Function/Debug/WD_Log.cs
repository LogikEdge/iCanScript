using UnityEngine;
using System.Collections;

[WD_Class(Company="Infaunier", Package="Debug")]
public class WD_Log : WD_Function {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    [WD_InPort] public string   message= "";
    
    // ======================================================================
    // EXECUTION
    // ----------------------------------------------------------------------
    [WD_Function]
    public override void Evaluate() {
        if(message != null && message != "") {
            Debug.Log(message);
        }
    }
}
