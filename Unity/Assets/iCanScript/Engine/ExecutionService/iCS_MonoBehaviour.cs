﻿using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("")]
public class iCS_MonoBehaviour : MonoBehaviour {
    // ======================================================================
    // Storage
    // ----------------------------------------------------------------------
    [SerializeField]
    private iCS_Storage myStorage= null;
    public iCS_Storage Storage {
        get {
            if(myStorage == null) {
                myStorage= ScriptableObject.CreateInstance("iCS_Storage") as iCS_Storage;
                myStorage.name= name;
            }
            return myStorage;
        }
    }
    
    // ======================================================================
    // Storage Redirect
    // ----------------------------------------------------------------------
    public List<iCS_EngineObject> EngineObjects {
        get { return Storage.EngineObjects; }
    }
    public string GetFullName(iCS_EngineObject obj) {
        return Storage.GetFullName(obj);
    }
    public iCS_EngineObject GetParent(iCS_EngineObject obj) {
        return Storage.GetParent(obj);
    }
    public iCS_EngineObject GetSourceEndPort(iCS_EngineObject port) {
        return Storage.GetSourceEndPort(port);
    }
    public iCS_EngineObject GetSourcePort(iCS_EngineObject port) {
        return Storage.GetSourcePort(port);
    }
    public iCS_EngineObject GetParentNode(iCS_EngineObject obj) {
        return Storage.GetParentNode(obj);
    }
    public bool IsOutPackagePort(iCS_EngineObject port) {
        return Storage.IsOutPackagePort(port);
    }
    
 }
