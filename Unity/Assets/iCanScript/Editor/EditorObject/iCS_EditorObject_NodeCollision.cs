using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;
using Prefs=iCS_PreferencesController;

public partial class iCS_EditorObject {    
    // ======================================================================
    // Collision Functions
    // ----------------------------------------------------------------------
    // Resolves the collision between children.  "true" is returned if a
    // collision has occured.
    // ----------------------------------------------------------------------
    public void ResolveCollisionOnChildrenNodes() {
		// Get a snapshot of the children state.
		var children= BuildListOfChildNodes(c=> !c.IsFloating);
        var childPos= P.map(n => n.LocalAnchorPosition+n.WrappingOffset, children);
		var childRect= P.map(n => BuildRect(n.LocalAnchorPosition+n.WrappingOffset, n.LayoutSize), children);
        // Resolve collisions.
        ResolveCollisionOnChildrenImp(children, ref childRect);
        // Update child position.
		for(int i= 0; i < children.Length; ++i) {
            children[i].CollisionOffset= PositionFrom(childRect[i])-childPos[i];
		}
    }
    // ----------------------------------------------------------------------
    private void ResolveCollisionOnChildrenImp(iCS_EditorObject[] children, ref Rect[] childRect) {
        // Resolve collisions.
		int r= 0;
        bool didCollide= true;
		while(didCollide) {
			didCollide= false;
			iCS_EditorObject lowest= null;
	        for(int i= 0; i < children.Length-1; ++i) {
				var c1= children[i];
	            for(int j= i+1; j < children.Length; ++j) {
					var c2= children[j];
	                if(c1.ResolveCollisionBetweenTwoNodes(c2, ref childRect[i],
															  ref childRect[j])) {
					    didCollide= true;	
					}
					if(c1.LayoutPriority > c2.LayoutPriority) {
						lowest= c1;
					} else if(c2.LayoutPriority > c1.LayoutPriority) {
						lowest= c2;
					}
	            }
	        }
			if(++r > 10) {
				if(lowest == null || lowest.LayoutPriority <= 1) {
					break;
				}
				lowest.LayoutPriority= lowest.LayoutPriority-1;
				r= 0;
			}
		}
    }
    // ----------------------------------------------------------------------
    // Resolves collision between two nodes. "true" is returned if a collision
    // has occured.
    public bool ResolveCollisionBetweenTwoNodes(iCS_EditorObject theOtherNode,
												ref Rect myRect, ref Rect theOtherRect) {
        // Nothing to do if they don't collide.
        if(!DoesCollideWithMargins(myRect, theOtherRect)) {
			return false;
		}
        
        // Compute penetration.
        Vector2 penetration;
        penetration= GetSeperationVector2(theOtherNode, myRect, theOtherRect);
        if(Mathf.Abs(penetration.x) < 1.0f && Mathf.Abs(penetration.y) < 1.0f) {
			return false;
		}
		// Use Layout priority to determine which node to move.
		if(LayoutPriority == theOtherNode.LayoutPriority) {
            penetration*= 0.5f;
			theOtherRect.x+= penetration.x;
			theOtherRect.y+= penetration.y;
			myRect.x-= penetration.x;
			myRect.y-= penetration.y;
            return true;            			
		}
		if(LayoutPriority < theOtherNode.LayoutPriority) {
			theOtherRect.x+= penetration.x;
			theOtherRect.y+= penetration.y;
            return true;			
		}
		if(LayoutPriority > theOtherNode.LayoutPriority) {
			myRect.x-= penetration.x;
			myRect.y-= penetration.y;
    		return true;			
		}
        return false;
    }
    // ----------------------------------------------------------------------
    // Returns true if the given rectangle collides with the node.
    public static bool DoesCollideWithMargins(Rect r1, Rect r2) {
        return Math3D.DoesCollide(AddMargins(r1), r2);
    }
    // ----------------------------------------------------------------------
	// Returns the seperation vector of two colliding nodes.  The vector
	// returned is the smallest distance to remove the overlap.
	Vector2 GetSeperationVector2(iCS_EditorObject theOther, Rect myRect, Rect otherRect) {
        myRect= AddMargins(myRect);
		// No collision if X & Y distance of the enclosing rect is either
		// larger or higher then the total width/height.
        var intersection= Math3D.Intersection(myRect, otherRect);
        if(Math3D.IsSmallerOrEqual(intersection.width,0f) || Math3D.IsSmallerOrEqual(intersection.height, 0f)) {
            return Vector2.zero;
        }
		// A collision is detected.  The seperation vector is the
		// smallest distance to remove the overlap.  The separtion
		// must also respect the anchor position relationship
		// between the two overalpping nodes.
		var anchorSepDir= theOther.LocalAnchorPosition-LocalAnchorPosition;
        var normalizedAnchorSep= anchorSepDir.normalized;
        // Assume vertical relation if anchor diff vector under 12 degrees
        if(Math3D.IsSmaller(Mathf.Abs(normalizedAnchorSep.x), 0.25f)) {
            return new Vector2(0f, intersection.height*Mathf.Sign(normalizedAnchorSep.y));
        }
        // Assume horizontal relation if anchor diff vector under 12 degrees
        if(Math3D.IsSmaller(Mathf.Abs(normalizedAnchorSep.y), 0.25f)) {
            return new Vector2(intersection.width*Mathf.Sign(normalizedAnchorSep.x), 0f);
        }
        var scaleX= Mathf.Abs(intersection.width / normalizedAnchorSep.x);
        var scaleY= Mathf.Abs(intersection.height / normalizedAnchorSep.y);
        var scale= Mathf.Min(scaleX, scaleY);
        return scale*normalizedAnchorSep;
	}

	// ======================================================================
	// Layout priority functionality.
	// ----------------------------------------------------------------------
	// Reduces the layout priority of all chidren.
	void ReduceChildrenLayoutPriority() {
		ForEachChildNode(c=> ++c.LayoutPriority);
	}
    // ----------------------------------------------------------------------
	// Sets the current object as the highest layout priority.
	public void SetAsHighestLayoutPriority() {
		var parent= ParentNode;
		if(parent != null) parent.ReduceChildrenLayoutPriority();
		LayoutPriority= 0;
		if(parent != null) parent.SetAsHighestLayoutPriority();
	}
}
