using UnityEngine;
using System.Collections;
using System.Reflection;

public sealed class WD_InFieldPort : WD_FieldPort {
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public static WD_InFieldPort CreateInstance(string portName, WD_Node parent) {
        WD_InFieldPort instance= CreateInstance<WD_InFieldPort>();
        instance.Init(portName, parent);
        return instance;
    }
    // ----------------------------------------------------------------------
    public override void Init(string thePortName, WD_Aggregate theParent) {
        base.Init(thePortName, theParent);

        // Allow streams to also be used as non-stream ports.
        if(IsStream) {
            FieldInfo fieldInfo= Parent.GetType().GetField(Name);
            System.Type fieldType= fieldInfo.FieldType;
            System.Array array= fieldInfo.GetValue(Parent) as System.Array;
            if(array == null || array.Length == 0) fieldInfo.SetValue(Parent, System.Array.CreateInstance(fieldType.GetElementType(), 1));
        }
    }
}
