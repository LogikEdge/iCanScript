using UnityEngine;
using System.Collections;

[WD_Class(Company="Infaunier", Package="Math3D")]
public class WD_FromVector3 : WD_Function {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    [WD_InPort]  public Vector3[]   vs;
    [WD_OutPort] public float[]     xs;
    [WD_OutPort] public float[]     ys;    
    [WD_OutPort] public float[]     zs;


    // ======================================================================
    // EXECUTION
    // ----------------------------------------------------------------------
    [WD_Function]
    public override void Evaluate() {
        xs= Prelude.map_(xs, (v)=> v.x, vs);
        ys= Prelude.map_(ys, (v)=> v.y, vs);
        zs= Prelude.map_(zs, (v)=> v.z, vs);
    }

}
