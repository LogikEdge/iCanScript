using UnityEngine;
using System.Collections;

public sealed class AP_LerpVector4 : AP_Function {
    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    [AP_InPort]  public Vector4[] xs;
    [AP_InPort]  public Vector4[] ys;
    [AP_InPort]  public float[] ratios;
    [AP_OutPort] public Vector4[] os;
    
    
    // ======================================================================
    // INITIALIZATION
    // ----------------------------------------------------------------------
    public static AP_LerpVector4 CreateInstance(string theFunctionName, AP_Node theParent) {
        AP_LerpVector4 instance= CreateInstance<AP_LerpVector4>();
        instance.Init(theFunctionName, theParent);
        return instance;
    }
    
    // ======================================================================
    // EXECUTION
    // ----------------------------------------------------------------------
    protected override void Evaluate() {
        os= Prelude.zipWith_(os, (x,y,ratio)=> x+(y-x)*ratio, xs, ys, ratios);
    }
}
