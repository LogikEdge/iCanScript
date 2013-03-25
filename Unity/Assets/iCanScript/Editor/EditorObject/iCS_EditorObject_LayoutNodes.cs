using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
//  NODE LAYOUT
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
public partial class iCS_EditorObject {
    // ----------------------------------------------------------------------
    // Layout the nodes from the parent of the object moving up the hierarchy
    // until we reach the top.  The sticky bit is carried over from the object
    // to the parent.
    public void LayoutParentNodesUntilTop(iCS_AnimationControl animCtrl= iCS_AnimationControl.Normal) {
        var parent= ParentNode;
        if(parent == null) return;
        var parentGlobalRect= parent.LayoutRect;
        parent.LayoutNode(animCtrl);
        if(Math3D.IsNotEqual(parentGlobalRect, parent.LayoutRect)) {
            parent.LayoutParentNodesUntilTop(animCtrl);
        }
    }
    // ----------------------------------------------------------------------
	public void LayoutNode(iCS_AnimationControl animCtrl= iCS_AnimationControl.Normal) {
        // Nothing to do for invisible ports.
        if(!IsVisibleInLayout) return;
        // Just update the size of the node if it is iconized.
        if(IsIconizedInLayout) {
            LayoutSize= iCS_Graphics.GetMaximizeIconSize(this);
            return;
        }
        // Resolve any existing collisions on children for unfolded modules.
        if(IsUnfoldedInLayout && !IsFunction) {
            if(IsAnimated) {
                ResolveCollisionOnLayoutChildrenNodes(iCS_AnimationControl.Always);
                WrapAroundLayoutChildrenNodes();                                
                if(Math3D.IsNotEqual(LayoutRect, AnimationTarget)) {
                    Animate(AnimatedRect, LayoutRect);
                }
            } else {
                ResolveCollisionOnAnimatedChildrenNodes(animCtrl);
                WrapAroundAnimatedChildrenNodes();                                
            }
    		return;            
        }
        // Update the size and ports for folded & Function nodes.
        LayoutRect= FoldedNodeRect();
	}

    // ----------------------------------------------------------------------
    // Updates the global Rect arround the children nodes.  It is assume that
    // the children have previously been layed out.  The anchor position and
    // layout size will be updated accordingly.
    // NOTE: This function must not be called for iconized nodes.
    // ----------------------------------------------------------------------
    public void WrapAroundLayoutChildrenNodes() {
        WrapAroundChildrenNodes(true);
    }
    // ----------------------------------------------------------------------
    public void WrapAroundAnimatedChildrenNodes() {
        WrapAroundChildrenNodes(false);
    }
    // ----------------------------------------------------------------------
    public void WrapAroundChildrenNodes(bool useLayout) { 
		// Nothing to do if node is not visible.
		if(!IsVisibleOnDisplay || IsIconizedOnDisplay) {
		    return;
	    }
		// Take a snapshot of the children global position.
		var childAnchorPositions= new List<Vector2>();
		ForEachChildNode(
		    c=> {
		        var p= useLayout ? c.LayoutAnchorPosition : c.AnimatedAnchorPosition;
		        childAnchorPositions.Add(p);
	        }
		);
        // Determine node global layout.
        var r= useLayout ? UnfoldedLayoutNodeRect() : UnfoldedAnimatedNodeRect();
		// Update parent node anchor positions.
		var center= Math3D.Middle(r);
		AnchorPosition= center-LocalOffset;
		// Update layout size.
		LayoutSize= new Vector2(r.width, r.height);
		// Reposition child to maintain their global positions.
		int i= 0;
		ForEachChildNode(
		    c=> {
                if(useLayout) {
    		        c.LayoutAnchorPosition= childAnchorPositions[i];                    
                } else {
    		        c.AnimatedAnchorPosition= childAnchorPositions[i];
                }
				++i;
	        }
		);
    }

    // ----------------------------------------------------------------------
    Vector2 IconizedSize() {
        return iCS_Graphics.GetMaximizeIconSize(this);
    }
    // ----------------------------------------------------------------------
    Rect FoldedNodeRect() {
        return NodeRectFromChildrenRectWithMargins(new Rect(0,0,0,0));
    }

    // ----------------------------------------------------------------------
    // We assume that the children have already been properly layed out.
    Rect UnfoldedAnimatedNodeRect() {
        return NodeRectFromChildrenRectWithMargins(AnimatedChildRectWithMargins);        
    }
    // ----------------------------------------------------------------------
    // We assume that the children have already been properly layed out.
    Rect UnfoldedLayoutNodeRect() {
        return NodeRectFromChildrenRectWithMargins(LayoutChildRectWithMargins);        
    }
    
    // ----------------------------------------------------------------------
    Rect NodeRectFromChildrenRectWithMargins(Rect childRect) {
        // Get padding for all sides.
		float topPadding= NodeTopPadding;
		float bottomPadding= NodeBottomPadding;
		float leftPadding= NodeLeftPadding;
		float rightPadding= NodeRightPadding;
        // Determine size of node.
        float width = childRect.width+leftPadding+rightPadding;
        float height= childRect.height+topPadding+bottomPadding;
		// Assure minimum size for title and ports.
		var titleHeight= NodeTitleHeight;
		var titleWidth= NodeTitleWidth;
		var neededPortHeight= MinimumHeightForPorts;
		var neededPortWidth = MinimumWidthForPorts;
		var minHeight= titleHeight+neededPortHeight;
		var minWidth= Mathf.Max(titleWidth, neededPortWidth);
        // Readjust parent size & position.
        float xOffset= 0f;
        float yOffset= 0f;
        if(width < minWidth) {
            xOffset= 0.5f*(minWidth-width);
            width= minWidth;
        }
        if(height < minHeight) {
            yOffset= 0.5f*(minHeight-height);
            height= minHeight;
        }
		// Determine rect to wrap children.
        float x, y;
		if(Math3D.IsZero(childRect.width) || Math3D.IsZero(childRect.height)) {
            var pos= LayoutPosition;
		    x= pos.x-0.5f*width;
		    y= pos.y-0.5f*height;		    
		} else {
		    x= childRect.x-leftPadding-xOffset;
		    y= childRect.y-topPadding-yOffset;
		}
		var r= new Rect(x, y, width,height);
        return r;
    }
    
    
}

