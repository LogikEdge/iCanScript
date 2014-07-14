﻿using UnityEngine;
using System.Collections;
using P= Prelude;
using Prefs= iCS_PreferencesController;

public partial class iCS_EditorObject {   
	// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //                  POSITION MODIFYING OPERATIONS
    // ======================================================================
    // It is assumed that the position is modified in the following order:
    // 1) User drag / relocate
    // 2) Wrapping is performed on agregate nodes
    // 3) Collision is resolved.
    // ----------------------------------------------------------------------
    // @ Called when user moves a node or port
    public Vector2 UserDragPosition {
        set {
            CollisionOffset= Vector2.zero;
            var parent= ParentNode;
            if(parent == null) {
                LocalAnchorPosition= value;
            }
            LocalAnchorPosition= value-parent.GlobalPosition+parent.WrappingOffset;
        }
    }
    
    // ----------------------------------------------------------------------
    public Vector2 LocalAnchor {
        get {
            return LocalAnchorPosition;
        }
        set {
            LocalAnchorPosition= value;
        }
    }
    // ----------------------------------------------------------------------
    public Vector2 LocalAnchorFromGlobalPosition {
    	set {
			var parent= ParentNode;
			if(parent == null) {
				LocalAnchor= value;
				return;
			}
			LocalAnchor= value-parent.GlobalPosition;
			LocalOffset= Vector2.zero;
    	}
    }
    // ----------------------------------------------------------------------
    public Vector2 LocalOffsetFromGlobalPosition {
    	set {
    		var parent= ParentNode;
    		if(parent == null) {
    			LocalOffset= value-LocalAnchor;
    			return;
    		}
    		LocalOffset= value-parent.GlobalPosition-LocalAnchor;
    	}
    }
}
