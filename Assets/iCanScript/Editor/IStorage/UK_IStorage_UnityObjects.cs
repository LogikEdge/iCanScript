using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class UK_IStorage {
    // ======================================================================
    // Unity Object containement functions
    // ----------------------------------------------------------------------
    public bool IsValidUnityObject(int id) { return Storage.IsValidUnityObject(id); }
    // ----------------------------------------------------------------------
    public Object GetUnityObject(int id) {
        return Storage.GetUnityObject(id);        
    }
    // ----------------------------------------------------------------------
    public void SetUnityObject(int id, Object value) {
        Storage.SetUnityObject(id, value);
    }
    // ----------------------------------------------------------------------
    public T GetUnityObject<T>(int id) where T : Object {
        return GetUnityObject(id) as T;
    }    
    // ----------------------------------------------------------------------
    public int AddUnityObject(Object obj) {
        return Storage.AddUnityObject(obj);
    }
    // ----------------------------------------------------------------------
    public void RemoveUnityObject(int id) {
        SetUnityObject(id, null);
    }
    // ----------------------------------------------------------------------
    public object GetDefaultValue(UK_RuntimeDesc desc, int portId) {
        return desc.GetDefaultValue(portId, Storage);
    }
    // ----------------------------------------------------------------------
    public void SetDefaultValue(UK_RuntimeDesc desc, int portId, object obj) {
        desc.SetDefaultValue(portId, obj, Storage);
    }
}
