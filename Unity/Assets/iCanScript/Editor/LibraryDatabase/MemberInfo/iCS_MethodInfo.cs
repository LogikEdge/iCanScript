using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;

public class iCS_MethodInfo : iCS_MethodBaseInfo {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    public MethodBase   Method= null;

    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public override string MethodName {
        get { return Method.Name; }
    }
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_MethodInfo(iCS_ObjectTypeEnum objType, iCS_TypeInfo _classInfo,
                          string _name, string _description, string _iconPath,
                          iCS_Parameter[] _parameters, iCS_FunctionReturn _functionReturn,
						  iCS_StorageClass _storageClass, MethodBase _methodBase)
    : base(objType, _classInfo, _name, _description, _iconPath, _parameters, _functionReturn, _storageClass) {
        Method        = _methodBase;
    }

}