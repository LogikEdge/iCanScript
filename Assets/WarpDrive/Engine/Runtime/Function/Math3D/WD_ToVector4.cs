using UnityEngine;
using System.Collections;

[WD_Class(Company="Infaunier", Package="Math3D")]
public sealed class WD_ToVector4 : WD_Function {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    [WD_InPort]  public float[]      xs;
    [WD_InPort]  public float[]      ys;
    [WD_InPort]  public float[]      zs;
    [WD_InPort]  public float[]      ws;
    [WD_OutPort] public Vector4[]    vs;

    // ======================================================================
    // EXECUTION
    // ----------------------------------------------------------------------
    [WD_Function]
    public override void Evaluate() {
        vs= Prelude.zipWith_(vs, (x,y,z,w)=> new Vector4(x,y,z,w), xs, ys, zs, ws);
    }
}
