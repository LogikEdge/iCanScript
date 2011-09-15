using UnityEngine;
using System.Collections;

public sealed class WD_LerpInt : WD_Function {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    [WD_InPort]  public int[] xs;
    [WD_InPort]  public int[] ys;
    [WD_InPort]  public float[] ratios;
    [WD_OutPort] public int[] os;
    
    
    // ======================================================================
    // EXECUTION
    // ----------------------------------------------------------------------
    protected override void Evaluate() {
        os= Prelude.zipWith_(os, (x,y,ratio)=> (int)(x+(y-x)*ratio), xs, ys, ratios);
    }
}
