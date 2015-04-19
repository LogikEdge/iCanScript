using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using iCanScript.Engine;

public partial class iCS_IStorage {    
    // =========================================================================
    // Trigger Port
    // -------------------------------------------------------------------------
    public iCS_EditorObject CreateTriggerPort(int parentId) {
        var existingTriggerPort= GetTriggerPort(EditorObjects[parentId]);
        if(existingTriggerPort != null) return existingTriggerPort;
        iCS_EditorObject port= CreatePort(iCS_Strings.TriggerPort, parentId, typeof(bool), iCS_ObjectTypeEnum.TriggerPort, (int)iCS_PortIndex.Trigger);
        port.IsNameEditable= false;
        port.InitialPortValue= true;
        return port;
    }
    // -------------------------------------------------------------------------
    public bool HasTriggerPort(iCS_EditorObject node)  {
        return GetTriggerPort(node) != null;
    }
    // -------------------------------------------------------------------------
    public iCS_EditorObject GetTriggerPort(iCS_EditorObject node) {
        return FindInChildren(node, c=> c.IsTriggerPort);
    }

    // ======================================================================
    // Enable Ports
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreateEnablePort(int parentId) {
        iCS_EditorObject port= CreatePort(iCS_Strings.EnablePort, parentId, typeof(bool), iCS_ObjectTypeEnum.EnablePort, (int)GetNextAvailableEnablePortIndex(EditorObjects[parentId]));
        port.IsNameEditable= false;
        port.InitialPortValue= true;
        return port;
    }
    // -------------------------------------------------------------------------
    public bool HasEnablePort(iCS_EditorObject package) {
        return GetEnablePorts(package).Length != 0;
    }
    // -------------------------------------------------------------------------
    public iCS_EditorObject[] GetEnablePorts(iCS_EditorObject package) {
        return BuildFilteredListOfChildren(c=> c.IsEnablePort, package);
    }
    // -------------------------------------------------------------------------
    public int GetNextAvailableEnablePortIndex(iCS_EditorObject node) {
        var enables= CleanupEnablePorts(node);
        if(enables == null) return (int)iCS_PortIndex.EnablesStart;
        return (int)iCS_PortIndex.EnablesStart + enables.Length;
    }
    // -------------------------------------------------------------------------
    public iCS_EditorObject[] CleanupEnablePorts(iCS_EditorObject node) {
        var enables= GetEnablePorts(node);
        if(enables == null || enables.Length == 0) return null;
        for(int i= 0; i < enables.Length; ++i) {
            enables[i].PortIndex= (int)iCS_PortIndex.EnablesStart+i;
        }
        return enables;
    }
	
	
    // =========================================================================
    // Target Port
    // -------------------------------------------------------------------------
    /// Creates a target port on the given parent.
    ///
    /// @param parent The visual script parent node.
    /// @return The newly create target port object.
    ///
	public iCS_EditorObject CreateTargetPort(iCS_EditorObject parent) {
        if(parent == null) {
            Debug.LogWarning("iCanScript: Trying to create a target port on NULL parent.");
            return null;
        }
        return CreateTargetPort(parent.InstanceId);
	}
    // -------------------------------------------------------------------------
    /// Creates a target port on the given parent.
    ///
    /// @param parentId The visual script object ID of the parent.
    /// @return The newly create target port object.
    ///
	public iCS_EditorObject CreateTargetPort(int parentId) {
        if(!IsIdValid(parentId)) {
            Debug.LogWarning("iCanScript: Trying to create a target port on an invalid parent ID.");
            return null;
        }
        var parentNode= EditorObjects[parentId];
        var runtimeType= parentNode.RuntimeType;
        if(runtimeType == null || runtimeType == typeof(void)) {
            Debug.LogWarning("iCanScript: Trying to create a target port with an invalid runtime type.");
            return null;            
        }
        var portName= "Target";
		var port= CreatePort(portName, parentId, runtimeType,
							 iCS_ObjectTypeEnum.InFixDataPort, (int)iCS_PortIndex.Target);
		port.IsNameEditable= false;
		return port;
	}

    // -------------------------------------------------------------------------
    /// Retruns the target port attached to th egiven node.
    ///
    /// @param node The parent node of the target port to search for.
    /// @return The target port if found. _null_ otherwise.
    ///
    public iCS_EditorObject GetTargetPort(iCS_EditorObject node) {
        return FindInChildren(node, c=> c.IsTargetPort);
    }

    // -------------------------------------------------------------------------
    /// Determines if the given node has a target port.
    ///
    /// @param node The parent node of the target port to search for.
    /// @return _true_ if the target port is found. _false_ otherwise.
    ///
    public bool HasTargetPort(iCS_EditorObject node)  {
        return GetTargetPort(node) != null;
    }


    // =========================================================================
    // Self Port
    // ----------------------------------------------------------------------
    /// Creates a self port on the given parent.
    ///
    /// @param parent The visual script parent object.
    /// @return The newly create self port object.
    ///
    public iCS_EditorObject CreateSelfPort(iCS_EditorObject parent) {
        if(parent == null) {
            Debug.LogWarning("iCanScript: Trying to create a self port on a NULL parent.");
            return null;
        }
        return CreateSelfPort(parent.InstanceId);
    }
    // ----------------------------------------------------------------------
    /// Creates a self port on the given parent.
    ///
    /// @param parentId The visual script object ID of the parent.
    /// @return The newly create self port object.
    ///
    public iCS_EditorObject CreateSelfPort(int parentId) {
        if(!IsIdValid(parentId)) {
            Debug.LogWarning("iCanScript: Trying to create a self port on an invalid parent ID.");
            return null;
        }
        var parentNode= EditorObjects[parentId];
        var runtimeType= parentNode.RuntimeType;
        if(runtimeType == null || runtimeType == typeof(void)) {
            Debug.LogWarning("iCanScript: Trying to create a self port with an invalid runtime type.");
            return null;            
        }
        iCS_EditorObject port= CreatePort("Self", parentId, runtimeType,
                                          iCS_ObjectTypeEnum.OutProposedDataPort, (int)iCS_PortIndex.Self);
        port.IsNameEditable= false;
        return port;
    }
    // -------------------------------------------------------------------------
    /// Retruns the self port attached to th egiven node.
    ///
    /// @param node The parent node of the self port to search for.
    /// @return The self port if found. _null_ otherwise.
    ///
    public iCS_EditorObject GetSelfPort(iCS_EditorObject node) {
        return FindInChildren(node, c=> c.IsSelfPort);
    }
    // -------------------------------------------------------------------------
    /// Determines if the given node has a target port.
    ///
    /// @param node The parent node of the target port to search for.
    /// @return _true_ if the target port is found. _false_ otherwise.
    ///
    public bool HasSelfPort(iCS_EditorObject node)  {
        return GetSelfPort(node) != null;
    }
    
	
    // ======================================================================
    // Dynamic Ports
    // ----------------------------------------------------------------------
	public iCS_EditorObject CreateInDynamicDataPort(string name, int parentId, Type valueType) {
		var parent= EditorObjects[parentId];
		int index= GetNextDynamicOrProposedPortIndex(parent);
		return CreatePort(name, parentId, valueType, iCS_ObjectTypeEnum.InDynamicDataPort, index);
	}
    // ----------------------------------------------------------------------
	public iCS_EditorObject CreateOutDynamicDataPort(string name, int parentId, Type valueType) {
		var parent= EditorObjects[parentId];
		int index= GetNextDynamicOrProposedPortIndex(parent);
		return CreatePort(name, parentId, valueType, iCS_ObjectTypeEnum.OutDynamicDataPort, index);		
	}


    // ======================================================================
    // Proposed Ports
    // ----------------------------------------------------------------------
	public iCS_EditorObject CreateInProposedDataPort(string name, int parentId, Type valueType) {
		var parent= EditorObjects[parentId];
		int index= GetNextDynamicOrProposedPortIndex(parent);
		return CreatePort(name, parentId, valueType, iCS_ObjectTypeEnum.InProposedDataPort, index);
	}
    // ----------------------------------------------------------------------
	public iCS_EditorObject CreateOutProposedDataPort(string name, int parentId, Type valueType) {
		var parent= EditorObjects[parentId];
		int index= GetNextDynamicOrProposedPortIndex(parent);
		return CreatePort(name, parentId, valueType, iCS_ObjectTypeEnum.OutProposedDataPort, index);		
	}


    // ======================================================================
    // Common Creation
    // ----------------------------------------------------------------------
    public iCS_EditorObject CreatePort(string name, int parentId, Type valueType, iCS_ObjectTypeEnum portType, int index= -1) {
        int id= GetNextAvailableId();
        var parent= EditorObjects[parentId];
        if(index == -1) {
            if(portType == iCS_ObjectTypeEnum.TriggerPort) {
                index= (int)iCS_PortIndex.Trigger;
            } if(portType == iCS_ObjectTypeEnum.EnablePort) {
                index= GetNextAvailableEnablePortIndex(parent);
            }
            else {
        		index= GetNextDynamicOrProposedPortIndex(parent);                
            }
        }
        iCS_EditorObject port= iCS_EditorObject.CreateInstance(id, name, valueType, parentId, portType, this);
        port.PortIndex= index;
        port.PortIterationSignature= PortIterationSignatureEnum.Dont_iterate;
        if(parent.IsPort) {
//            port.LocalOffset= parent.LocalOffset;
            port.CollisionOffset= parent.CollisionOffset;
        } else {
            var globalPos= parent.GlobalPosition;
//    		port.GlobalPosition= globalPos;            
            port.CollisionOffsetFromGlobalPosition= globalPos;
        }
		// Set initial port edge.
		if(port.IsEnablePort) {
			port.Edge= iCS_EdgeEnum.Top;
		} else if(port.IsTriggerPort) {
			port.Edge= iCS_EdgeEnum.Bottom;
		} else if(port.IsInputPort) {
			port.Edge= iCS_EdgeEnum.Left;
		} else if(port.IsDataPort) {
			port.Edge= iCS_EdgeEnum.Right;
		} else {
			port.UpdatePortEdge();			
		}
		port.CleanupPortEdgePosition();
        return EditorObjects[id];        
    }


    // ======================================================================
    // Creation
    // ----------------------------------------------------------------------
    public void MoveDynamicPortToLastIndex(iCS_EditorObject port) {
		// Relocate the port at the end.
        iCS_EditorObject parent= port.Parent;
		port.PortIndex= RecalculatePortIndexes(parent).Length;
        // Rearrange port indexes
        RecalculatePortIndexes(parent);
    }

	// ======================================================================
	// High-order functions
    // ----------------------------------------------------------------------
	public iCS_EditorObject[] RecalculatePortIndexes(iCS_EditorObject node) {
		List<iCS_EditorObject> ports= new List<iCS_EditorObject>();
		// Get all child data ports.
		ForEachChildDataPort(node, child=> ports.Add(child));
		// Sort child ports according to index.
		iCS_EditorObject[] result= SortPortsOnIndex(ports.ToArray());
		// Find first dynamic or proposed port
		int firstDynamicIdx= 0;
		for(int i= 0; i < result.Length; ++i) {
		    if(result[i].IsParameterPort && result[i].IsFixDataPort) {
		        firstDynamicIdx= result[i].PortIndex+1;
		    }
		}
		// Re-index dynamic and proposed ports
        for(int i= 0; i < result.Length; ++i) {
            if(result[i].IsDynamicDataPort || result[i].IsProposedDataPort) {
                if(result[i].PortIndex <= (int)iCS_PortIndex.ParametersEnd) {
                    result[i].PortIndex= firstDynamicIdx++; 
                }
            }
        }
		return result;
	}
    // -------------------------------------------------------------------------
    public int GetNextDynamicOrProposedPortIndex(iCS_EditorObject node) {
        var ports= RecalculatePortIndexes(node);
        int lastIdx= 0;
        for(int i= 0; i < ports.Length; ++i) {
            var p= ports[i];
            if(p.IsDynamicDataPort || p.IsProposedDataPort) {
                lastIdx= p.PortIndex+1;
            }
        }
        return lastIdx;
    }
    // ----------------------------------------------------------------------
    public static iCS_EditorObject[] SortPortsOnIndex(iCS_EditorObject[] lst) {
        // Find largest dynamic or proposed port.
        int lastIndex= -1;
        foreach(var p in lst) {
			if(p.IsDynamicDataPort || p.IsProposedDataPort) {
	            if(p.PortIndex > lastIndex) {
	                lastIndex= p.PortIndex;
	            }				
			}
        }
        // Assign all unassigned port indexes (we assume that it is a dynmic port).
        if(lastIndex != -1) {
            foreach(var p in lst) {
                if(p.PortIndex < 0) {
                    p.PortIndex= ++lastIndex;
                }
            }
        }
		Array.Sort(lst, (x,y)=> x.PortIndex - y.PortIndex);
        return lst;
    }

}
