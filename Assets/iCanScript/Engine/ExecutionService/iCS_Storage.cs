using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// This class is the main storage of uCode.  All object are derived
// from this storage class.
public class iCS_Storage : MonoBehaviour {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    /*[HideInInspector]*/ public List<iCS_EngineObject>   EngineObjects = new List<iCS_EngineObject>();
    [HideInInspector] public List<Object>             UnityObjects  = new List<Object>();
    [HideInInspector] public int                      UndoRedoId    = 0;
	[HideInInspector] public Vector2				  ScrollPosition= Vector2.zero;
	[HideInInspector] public float  				  GuiScale      = 1f;	
	[HideInInspector] public int    				  SelectedObject= -1;	

    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public bool IsValidEngineObject(int id) { return id >= 0 && id < EngineObjects.Count && EngineObjects[id] != null; }
    public bool IsValidUnityObject(int id)  { return id >= 0 && id < UnityObjects.Count && UnityObjects[id] != null; }

    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
	void OnEnable()  { Debug.Log("Enabling storage"); }
	void OnDisable() { Debug.Log("Disabling storage"); }
	
    // ----------------------------------------------------------------------
    public bool IsMuxPortChild(int id)  {
        if(!IsValidEngineObject(id)) return false;
        iCS_EngineObject eObj= EngineObjects[id];
        return eObj.IsInModulePort && GetParent(eObj).IsOutModulePort;
    }
    
    // ======================================================================
    // UnityObject Utilities
    // ----------------------------------------------------------------------
    public void ClearUnityObjects() {
        UnityObjects.Clear();
    }
    // ----------------------------------------------------------------------
    public int AddUnityObject(Object obj) {
        if(obj == null) return -1;
		// Search for an existing entry.
        int id= 0;
		int availableSlot= -1;
		for(id= 0; id < UnityObjects.Count; ++id) {
			if(UnityObjects[id] == obj) {
				return id;
			}
			if(UnityObjects[id] == null) {
				availableSlot= id;
			}
		}
		if(availableSlot != -1) {
			UnityObjects[availableSlot]= obj;
			return availableSlot;
		}
        UnityObjects.Add(obj);
        return id;
    }
    // ----------------------------------------------------------------------
    public Object GetUnityObject(int id) {
        return (id >= 0 && id < UnityObjects.Count) ? UnityObjects[id] : null;
    }

    // ======================================================================
    // EnginObject Utilities
    // ----------------------------------------------------------------------
    public iCS_EngineObject GetParent(iCS_EngineObject child) {
        if(child == null || child.ParentId == -1) return null;
        return EngineObjects[child.ParentId]; 
    }
    // ----------------------------------------------------------------------
    public iCS_EngineObject GetSource(iCS_EngineObject port) {
        if(port == null || port.SourceId == -1) return null;
        return EngineObjects[port.SourceId];
    }
    // ----------------------------------------------------------------------
    // Returns the absolute position of the node.
    public Rect GetPosition(iCS_EngineObject node) {
        if(!IsValidEngineObject(node.ParentId)) return node.LocalPosition;
        Rect position= GetPosition(EngineObjects[node.ParentId]);
        return new Rect(position.x+node.LocalPosition.x,
                        position.y+node.LocalPosition.y,
                        node.LocalPosition.width,
                        node.LocalPosition.height);
    }
    // ----------------------------------------------------------------------
    // Returns the last data port in the connection or NULL if none exist.
    public iCS_EngineObject GetDataConnectionSource(iCS_EngineObject port) {
        if(port == null || !port.IsDataPort) return null;
        for(iCS_EngineObject sourcePort= GetSource(port); sourcePort != null && sourcePort.IsDataPort; sourcePort= GetSource(port)) {
            port= sourcePort;
        }
        return port;
    }
    
}
