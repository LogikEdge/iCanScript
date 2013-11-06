using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO : Should storage be changed to a scriptable object ?
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// This class is the main storage of iCanScript.  All object are derived
// from this storage class.
[AddComponentMenu("")]
public class iCS_Storage : MonoBehaviour {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
                      public iCS_EngineObject         EngineObject       = null;
    [HideInInspector] public int			          MajorVersion       = iCS_Config.MajorVersion;
    [HideInInspector] public int    		          MinorVersion       = iCS_Config.MinorVersion;
    [HideInInspector] public int    		          BugFixVersion      = iCS_Config.BugFixVersion;
    [HideInInspector] public int                      UndoRedoId         = 0;
	[HideInInspector] public Vector2		          ScrollPosition     = Vector2.zero;
	[HideInInspector] public float  		          GuiScale           = 1f;	
	[HideInInspector] public int    		          SelectedObject     = -1;	
    [HideInInspector] public List<Object>             UnityObjects       = new List<Object>();
    [HideInInspector] public List<iCS_EngineObject>   EngineObjects      = new List<iCS_EngineObject>();

    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public bool IsValidEngineObject(int id) {
		return id >= 0 && id < EngineObjects.Count && EngineObjects[id].InstanceId != -1;
	}
    public bool IsValidUnityObject(int id)  {
		return id >= 0 && id < UnityObjects.Count && UnityObjects[id] != null;
	}

    // ======================================================================
    // Unity Object Utilities
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
    // Tree Navigation Queries
    // ----------------------------------------------------------------------
    public iCS_EngineObject GetParent(iCS_EngineObject child) {
        if(child == null || child.ParentId == -1) return null;
        return EngineObjects[child.ParentId]; 
    }
    // ----------------------------------------------------------------------
	public iCS_EngineObject GetParentNode(iCS_EngineObject child) {
		var parentNode= GetParent(child);
		while(parentNode != null && !parentNode.IsNode) {
			parentNode= GetParent(parentNode);
		}
		return parentNode;
	}
    // ----------------------------------------------------------------------
	public string GetFullPathName(iCS_EngineObject obj) {
		if(obj == null) return "";
		string fullName= null;
		for(fullName= obj.Name; obj != null; obj= GetParentNode(obj)) {
			fullName+= "::"+obj.Name;
		}
		return fullName;
	}
	
    // ======================================================================
    // Connection Queries
    // ----------------------------------------------------------------------
    // Returns the immediate source of the port.
    public iCS_EngineObject GetSourcePort(iCS_EngineObject port) {
        if(port == null || port.SourceId == -1) return null;
        return EngineObjects[port.SourceId];
    }
    // ----------------------------------------------------------------------
    // Returns the endport source of a connection.
    public iCS_EngineObject GetSourceEndPort(iCS_EngineObject port) {
        if(port == null) return null;
        int linkLength= 0;
        for(iCS_EngineObject sourcePort= GetSourcePort(port); sourcePort != null; sourcePort= GetSourcePort(port)) {
            port= sourcePort;
            if(++linkLength > 1000) {
                Debug.LogWarning("iCanScript: Circular port connection detected on: "+GetParentNode(port).Name+"."+port.Name);
                return null;                
            }
        }
        return port;
    }
    // ----------------------------------------------------------------------
    // Returns the list of destination ports.
    public iCS_EngineObject[] GetDestinationPorts(iCS_EngineObject port) {
        if(port == null) return new iCS_EngineObject[0];
        var destinationPorts= new List<iCS_EngineObject>();
        foreach(var obj in EngineObjects) {
            if(obj.IsPort && GetSourcePort(obj) == port) {
                destinationPorts.Add(obj);
            }
        }
        return destinationPorts.ToArray();
    }
    // ----------------------------------------------------------------------
    public bool IsEndPort(iCS_EngineObject port) {
        if(port == null) return false;
        if(!HasASource(port)) return true;
        return !HasADestination(port);
    }
    // ----------------------------------------------------------------------
    public bool IsRelayPort(iCS_EngineObject port) {
        if(port == null) return false;
        return HasASource(port) && HasADestination(port);
    }
    // ----------------------------------------------------------------------
    public bool HasASource(iCS_EngineObject port) {
        var source= GetSourcePort(port);
        return source != null && source != port; 
    }
    // ----------------------------------------------------------------------
    public bool HasADestination(iCS_EngineObject port) {
        return GetDestinationPorts(port).Length != 0;
    }
    
    // ======================================================================
    // EnginObject Utilities
    // ----------------------------------------------------------------------
    // ----------------------------------------------------------------------
	public bool IsInPackagePort(iCS_EngineObject obj) {
		if(!obj.IsInDataOrControlPort) return false;
		var parent= GetParentNode(obj);
		return parent != null && parent.IsKindOfPackage;
	}
    // ----------------------------------------------------------------------
	public bool IsOutPackagePort(iCS_EngineObject obj) {
		if(!obj.IsOutDataOrControlPort) return false;
		var parent= GetParentNode(obj);
		return parent != null && parent.IsKindOfPackage;
	}
    
}
