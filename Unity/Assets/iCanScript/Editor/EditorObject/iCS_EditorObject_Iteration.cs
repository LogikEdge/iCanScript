using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
//  ITERATION
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
public partial class iCS_EditorObject {
	// Child Queries ========================================================
	public bool HasChildNode() {
        foreach(var childId in Children) {
            if(EditorObjects[childId].IsNode) return true;
        }
		return false;
	}
    // ----------------------------------------------------------------------
	public bool HasChildPort() {
        foreach(var childId in Children) {
            if(EditorObjects[childId].IsPort) return true;
        }
		return false;
	}

    // Children Iterations =================================================
    public void ForEachChild(Action<iCS_EditorObject> fnc) {
        foreach(var childId in Children) {
            fnc(EditorObjects[childId]);
        }
    }
    // ----------------------------------------------------------------------
    public bool UntilMatchingChild(Func<iCS_EditorObject,bool> fnc) {
        foreach(var childId in Children) {
            if(fnc(EditorObjects[childId])) return true;
        }
        return false;
    }

    // Node Iterations ======================================================
    public void ForEachChildNode(Action<iCS_EditorObject> action) {
        ForEachChild(o=> { if(o.IsNode) action(o); });
    }
	// ----------------------------------------------------------------------
    public int NbOfChildNodes {
		get {
			int cnt= 0;
			ForEachChildNode(_=> ++cnt);
			return cnt;
		}
	}

    // Port Iterations ======================================================
    public void ForEachChildPort(Action<iCS_EditorObject> action) {
        ForEachChild(o=> { if(o.IsPort) action(o); });
    }
    // ----------------------------------------------------------------------
    public void ForEachLeftChildPort(Action<iCS_EditorObject> action) {
        ForEachChildPort(o=> { if(o.IsOnLeftEdge && !o.IsFloating) action(o); });
    }
    // ----------------------------------------------------------------------
    public void ForEachRightChildPort(Action<iCS_EditorObject> action) {
        ForEachChildPort(o=> { if(o.IsOnRightEdge && !o.IsFloating) action(o); });
    }
    // ----------------------------------------------------------------------
    public void ForEachTopChildPort(Action<iCS_EditorObject> action) {
        ForEachChildPort(o=> { if(o.IsOnTopEdge && !o.IsFloating) action(o); });
    }
    // ----------------------------------------------------------------------
    public void ForEachBottomChildPort(Action<iCS_EditorObject> action) {
        ForEachChildPort(o=> { if(o.IsOnBottomEdge && !o.IsFloating) action(o); });
    }
	// ----------------------------------------------------------------------
    public int NbOfTopPorts {
		get {
			int cnt= 0;
			ForEachTopChildPort(_=> ++cnt);
			return cnt;
		}
	}
	// ----------------------------------------------------------------------
    public int NbOfBottomPorts {
		get {
			int cnt= 0;
			ForEachBottomChildPort(_=> ++cnt);
			return cnt;
		}
	}
	// ----------------------------------------------------------------------
    public int NbOfLeftPorts {
		get {
			int cnt= 0;
			ForEachLeftChildPort(_=> ++cnt);
			return cnt;
		}
	}
	// ----------------------------------------------------------------------
    public int NbOfRightPorts {
		get {
			int cnt= 0;
			ForEachRightChildPort(_=> ++cnt);
			return cnt;
		}
	}
	// ----------------------------------------------------------------------
    public iCS_EditorObject[] TopPorts {
		get {
			return BuildListOfChildPorts(c=> c.IsOnTopEdge && !c.IsFloating);
		}
	}
	// ----------------------------------------------------------------------
    public iCS_EditorObject[] BottomPorts {
		get {
			return BuildListOfChildPorts(c=> c.IsOnBottomEdge && !c.IsFloating);
		}
	}
	// ----------------------------------------------------------------------
    public iCS_EditorObject[] LeftPorts {
		get {
			return BuildListOfChildPorts(c=> c.IsOnLeftEdge && !c.IsFloating);
		}
	}
	// ----------------------------------------------------------------------
    public iCS_EditorObject[] RightPorts {
		get {
			return BuildListOfChildPorts(c=> c.IsOnRightEdge && !c.IsFloating);
		}
	}
	    
    // Recursive Iterations =================================================
    public void ForEachRecursiveDepthFirst(Action<iCS_EditorObject> fnc) {
        ForEachRecursiveDepthFirst(_=> true, fnc);
    }
	// ----------------------------------------------------------------------
    public void ForEachRecursiveDepthFirst(Func<iCS_EditorObject, bool> cond,
                                           Action<iCS_EditorObject> fnc) {
        // Does this node pass the given condition?
        var editorObject= EditorObject;
        if(!cond(editorObject)) return;
        // First iterate through all children ...
        foreach(var childId in Children) {
			if(childId != -1) {
				var child= EditorObjects[childId];
				if(child != null) {
		            child.ForEachRecursiveDepthFirst(fnc);									
				} else {
					Debug.LogWarning("iCanScript: Mismatch between children list and EditorObject container !!!");
				}
			} else {
				Debug.LogWarning("iCanScript: Children list includes an invalid id");
			}
        }
        // ... then this node.
        fnc(editorObject);
    }
	// ----------------------------------------------------------------------
    public void ForEachRecursiveDepthLast(Action<iCS_EditorObject> fnc) {
        ForEachRecursiveDepthLast(_=> true, fnc);
    }
	// ----------------------------------------------------------------------
    public void ForEachRecursiveDepthLast(Func<iCS_EditorObject,bool> cond,
                                          Action<iCS_EditorObject> fnc) {
        // Does this node pass the given condition?
        var editorObject= EditorObject;
        if(!cond(editorObject)) return;
        // First this node ...
        fnc(editorObject);
        // ... then iterate through all children.
        foreach(var childId in Children) {
			if(childId != -1) {
				var child= EditorObjects[childId];
				if(child != null) {
		            child.ForEachRecursiveDepthLast(fnc);									
				} else {
					Debug.LogWarning("iCanScript: Mismatch between children list and EditorObject container !!!");
				}
			} else {
				Debug.LogWarning("iCanScript: Children list includes an invalid id");
			}
        }
    }
    // ----------------------------------------------------------------------
    // Recursively iterating through all child nodes invoking the given
    // function on the leaf node first then the branch nodes.
    public void ForEachChildRecursiveDepthFirst(Action<iCS_EditorObject> fnc) {
        ForEachChildRecursiveDepthFirst(_=> true, fnc);
    }
    // ----------------------------------------------------------------------
    // Recursively iterating through all child nodes invoking the given
    // function on the leaf node first then the branch nodes.
    public void ForEachChildRecursiveDepthFirst(Func<iCS_EditorObject,bool> cond,
                                                Action<iCS_EditorObject> fnc) {
        // Iterate through all children ...
        foreach(var childId in Children) {
			if(childId != -1) {
				var child= myIStorage[childId];
				if(child != null) {
		            child.ForEachRecursiveDepthFirst(cond, fnc);
				} else {
					Debug.LogWarning("iCanScript: Mismatch between children list and EditorObject container !!!");
				}
			} else {
				Debug.LogWarning("iCanScript: Children list includes an invalid id");
			}
        }
    }
    // ----------------------------------------------------------------------
    // Recursively iterating through all child nodes invoking the given
    // function on the branch node first then the leaf nodes.
    public void ForEachChildRecursiveDepthLast(Action<iCS_EditorObject> fnc) {
        ForEachChildRecursiveDepthLast(_=> true, fnc);
    }
    // ----------------------------------------------------------------------
    // Recursively iterating through all child nodes invoking the given
    // function on the branch node first then the leaf nodes.
    public void ForEachChildRecursiveDepthLast(Func<iCS_EditorObject,bool> cond,
                                               Action<iCS_EditorObject> fnc) {
        // Iterate through all children.
        foreach(var childId in Children) {
			if(childId != -1) {
				var child= myIStorage[childId];
				if(child != null) {
            		child.ForEachRecursiveDepthLast(cond, fnc);
				} else {
					Debug.LogWarning("iCanScript: Mismatch between children list and EditorObject container !!!");
				}
			} else {
				Debug.LogWarning("iCanScript: Children list includes an invalid id");
			}
        }
    }

    // ======================================================================
	// List builders.
    // ----------------------------------------------------------------------
	// Build list of children that satisfies the given criteria.
    public iCS_EditorObject[] BuildListOfChildren(Func<iCS_EditorObject, bool> cond) {
        var result= new List<iCS_EditorObject>();
        ForEachChild(c=> { if(cond(c)) result.Add(c); });
        return result.ToArray();
    }
    // ----------------------------------------------------------------------
    // Build a list of child nodes that satisfies the given criteria.
	public iCS_EditorObject[] BuildListOfChildNodes(Func<iCS_EditorObject, bool> cond) {
		return BuildListOfChildren(c=> c.IsNode && cond(c));
	}
    // ----------------------------------------------------------------------
    // Build a list of ports that satisfies the given criteria.
	public iCS_EditorObject[] BuildListOfChildPorts(Func<iCS_EditorObject, bool> cond) {
		return BuildListOfChildren(c=> c.IsPort && cond(c));
	}
    // ----------------------------------------------------------------------
    // Build a list of ports on the same edge.
    public iCS_EditorObject[] BuildListOfPortsOnSameEdge() {
        Func<iCS_EditorObject,bool> cond= null;
        switch(Edge) {
            case iCS_EdgeEnum.Left:     cond= p=> p.IsOnLeftEdge;   break;
            case iCS_EdgeEnum.Right:    cond= p=> p.IsOnRightEdge;  break;
            case iCS_EdgeEnum.Top:      cond= p=> p.IsOnTopEdge;    break;
            case iCS_EdgeEnum.Bottom:   cond= p=> p.IsOnBottomEdge; break;
            default: break;
        }
        if(cond == null) return new iCS_EditorObject[0];
        return ParentNode.BuildListOfChildPorts(cond);
    }
}
