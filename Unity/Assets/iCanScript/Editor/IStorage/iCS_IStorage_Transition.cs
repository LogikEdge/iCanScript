using UnityEngine;
using System.Collections;

public partial class iCS_IStorage {    
    // ======================================================================
    // Creation methods
    // ----------------------------------------------------------------------
    // Updates the port names of a transition.
    public void UpdatePortNames(iCS_EditorObject fromStatePort, iCS_EditorObject toStatePort) {
        // State ports
        var fromParent= fromStatePort.Parent;
        var toParent  = toStatePort.Parent;
        string statePortName= fromParent.Name+"->"+toParent.Name;
        fromStatePort.Name= statePortName;
        toStatePort.Name  = statePortName;
        fromStatePort.IsNameEditable= false;
        toStatePort.IsNameEditable= false;
        // Transition module ports.
        var transitionPackage = GetTransitionPackage(toStatePort);
        var inTransitionPort = GetInTransitionPort(transitionPackage);
        var outTransitionPort= GetOutTransitionPort(transitionPackage);
        inTransitionPort.Name= fromParent.Name+"->"+transitionPackage.Name;
        outTransitionPort.Name= transitionPackage.Name+"->"+toParent.Name;
        inTransitionPort.IsNameEditable= false;
        outTransitionPort.IsNameEditable= false;
    }
    // ----------------------------------------------------------------------
    // Returns the common parent of given states.
    public iCS_EditorObject GetTransitionParent(iCS_EditorObject toState, iCS_EditorObject fromState) {
        bool parentFound= false;
        iCS_EditorObject fromParent= null;
        for(fromParent= fromState; fromParent != null; fromParent= fromParent.Parent) {
            iCS_EditorObject toParent= null;
            for(toParent= toState; toParent != null; toParent= toParent.Parent) {
                if(fromParent == toParent) {
                    parentFound= true;
                    break;
                }
            }
            if(parentFound) break;
        }
        return fromParent;        
    }
    
    // ======================================================================
    // Transition helpers.
    // ----------------------------------------------------------------------
    public iCS_EditorObject GetFromStatePort(iCS_EditorObject transitionObject) {
		if(transitionObject == null) {
			Debug.LogWarning("iCanScript: Trying to get transition source port with a NULL object");
			return null;
		}
        if(transitionObject.IsInStatePort) {
        	iCS_EditorObject source= transitionObject.Source;
			if(source == null) {
				Debug.LogWarning("iCanScript: State Transition destination port not connected on state: "+transitionObject.ParentNode.Name);
				return null;
			}
            if(source.IsOutStatePort) return source;
            transitionObject= source.Parent;
        }
        if(transitionObject.IsTransitionPackage) {
            if(!UntilMatchingChildPort(transitionObject,
                child=> {
                    if(child.IsInTransitionPort) {
                        transitionObject= child;
                        return true;
                    }
                    return false;
                }
            )) {
				Debug.LogWarning("iCanScript: Unable to find transition input port on Transition package");
				return null;            	
            }
        }
        if(transitionObject.IsInTransitionPort) {
        	iCS_EditorObject source= transitionObject.Source;
			if(source == null) {
				Debug.LogWarning("iCanScript: Transition package input port not connected to a state");
				return null;
			}
			transitionObject= source;
        }
        if(transitionObject.IsOutStatePort) return transitionObject;
		Debug.LogWarning("iCanScript: State Transition destination port not connected on state: "+transitionObject.ParentNode.Name);
        return null;
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject GetToStatePort(iCS_EditorObject transitionObject) {
		if(transitionObject.IsOutStatePort) {
			transitionObject= FindAConnectedPort(transitionObject);
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsInTransitionPort) {
			transitionObject= transitionObject.ParentNode;
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsTransitionPackage) {
	        UntilMatchingChildPort(transitionObject,
	            p=> {
	                if(p.IsOutTransitionPort) {
	                    transitionObject= p;
	                    return true;
	                }
	                return false;
	            }
	        );
			if(!transitionObject.IsOutTransitionPort) return null;
		}
		if(transitionObject.IsOutTransitionPort) {
			return FindAConnectedPort(transitionObject);
		}
		return null;
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject GetInTransitionPort(iCS_EditorObject transitionObject) {
		if(transitionObject.IsOutStatePort) {
			transitionObject= FindAConnectedPort(transitionObject);
			if(transitionObject == null) return null;
		}
        if(transitionObject.IsInTransitionPort) return transitionObject;
		if(transitionObject.IsInStatePort) {
			transitionObject= transitionObject.Source;
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsOutTransitionPort) {
			transitionObject= transitionObject.ParentNode;
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsTransitionPackage) {
	        UntilMatchingChildPort(transitionObject,
	            p=> {
	                if(p.IsInTransitionPort) {
	                    transitionObject= p;
	                    return true;
	                }
	                return false;
	            }
	        );
			if(transitionObject.IsInTransitionPort) return transitionObject;
		}
		Debug.LogWarning("iCanScript: Input Transition port not found: "+transitionObject.Name);
		return null;
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject GetOutTransitionPort(iCS_EditorObject transitionObject) {
		if(transitionObject.IsInStatePort) {
			transitionObject= transitionObject.Source;
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsOutTransitionPort) return transitionObject;
		if(transitionObject.IsOutStatePort) {
			transitionObject= FindAConnectedPort(transitionObject);
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsInTransitionPort) {
			transitionObject= transitionObject.ParentNode;
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsTransitionPackage) {
	        UntilMatchingChildPort(transitionObject,
	            p=> {
	                if(p.IsOutTransitionPort) {
	                    transitionObject= p;
	                    return true;
	                }
	                return false;
	            }
	        );
			if(transitionObject.IsOutTransitionPort) return transitionObject;
		}
		return null;
    }
    // ----------------------------------------------------------------------
    public iCS_EditorObject GetTransitionPackage(iCS_EditorObject transitionObject) {
		if(transitionObject == null) return null;
		if(transitionObject.IsTransitionPackage) return transitionObject;
		if(transitionObject.IsInStatePort) {
			transitionObject= transitionObject.Source;
			if(transitionObject == null) return null;
		}
		if(transitionObject.IsOutStatePort) {
			transitionObject= FindAConnectedPort(transitionObject);
			if(transitionObject == null) return null;
		}
		transitionObject= transitionObject.ParentNode;
		if(transitionObject.IsTransitionPackage) return transitionObject;
		return null;
    }
    // ----------------------------------------------------------------------
    public Vector2 ProposeTransitionPackagePosition(iCS_EditorObject module) {
        iCS_EditorObject fromStatePort= GetFromStatePort(module);
        iCS_EditorObject toStatePort= GetToStatePort(module);
		if(toStatePort == null || fromStatePort ==null) return module.LayoutPosition;
        iCS_EditorObject parent= module.Parent;
		var startPos= fromStatePort.LayoutPosition;
		var endPos= toStatePort.LayoutPosition;
		var delta= endPos-startPos;
		var marginSize= iCS_EditorConfig.MarginSize; if(marginSize < 5) marginSize=5;
		Vector2 step= 0.5f*marginSize*(delta).normalized;
		var minPos= startPos;
		var maxPos= endPos;
		for(; Vector2.Distance(minPos, endPos) >= marginSize; minPos+= step) {
			if(GetNodeAt(minPos) == parent) {
				break;
			}
		}
		for(maxPos= minPos+step; Vector2.Distance(maxPos, endPos) >= marginSize; maxPos+= step) {
			if(GetNodeAt(maxPos) != parent) {
				break;
			}
		}
		return 0.5f*(minPos+maxPos);
    }
    // ----------------------------------------------------------------------
    public void LayoutTransitionPackage(iCS_EditorObject package) {
        package.SetAnchorAndLayoutPosition(ProposeTransitionPackagePosition(package));
    }
    // ----------------------------------------------------------------------
    public Vector2 GetTransitionPackageVector(iCS_EditorObject package) {
		// Preconditions.
		if(package == null) {
			Debug.LogWarning("iCanScript: Attempting to get Transition Package Vector with a NULL package");
			return Vector2.zero;
		}
        iCS_EditorObject inStatePort      = GetToStatePort(package);
        iCS_EditorObject outStatePort     = GetFromStatePort(package);
        iCS_EditorObject inTransitionPort = GetInTransitionPort(package);
        iCS_EditorObject outTransitionPort= GetOutTransitionPort(package);
        var inStatePos= inStatePort.LayoutPosition;
        var outStatePos= outStatePort.LayoutPosition;
        var inTransitionPos= inTransitionPort.LayoutPosition;
        var outTransitionPos= outTransitionPort.LayoutPosition;
        Vector2 dir= ((inStatePos-outTransitionPos).normalized+(inTransitionPos-outStatePos).normalized).normalized;
        return dir;
    }
}
