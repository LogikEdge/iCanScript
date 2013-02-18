/*
   iCS_EditorObject_DisplayState.cs
   iCanScript
   
   Created by Reinual on 2013-02-15.
   Copyright 2013 Infaunier. All rights reserved.
*/

using UnityEngine;
using System.Collections;

public partial class iCS_EditorObject {
    // ======================================================================
    // Queries
    // ----------------------------------------------------------------------    
    public bool IsUnfoldedInLayout  { get { return DisplayOption == iCS_DisplayOptionEnum.Unfolded; }}
    public bool IsFoldedInLayout    { get { return DisplayOption == iCS_DisplayOptionEnum.Folded;   }}
    public bool IsIconizedInLayout  { get { return DisplayOption == iCS_DisplayOptionEnum.Iconized; }}

    // ======================================================================
    // High-order queries.
	public bool IsUnfoldedOnDisplay { get { return IsUnfoldedInLayout; }}
    // ----------------------------------------------------------------------
	public bool IsFoldedOnDisplay	{ get { return IsFoldedInLayout; }}
    // ----------------------------------------------------------------------
	public bool IsIconizedOnDisplay	{
		get {
            if(IsPort) return false;
            var area= Math3D.Area(DisplaySize);
            if(Math3D.IsZero(area)) return false;
            var iconArea= Math3D.Area(iCS_Graphics.GetMaximizeIconSize(this));
            return Math3D.IsSmallerOrEqual(area, iconArea) &&
                   Math3D.IsGreater(area, iCS_EditorConfig.kMinIconicArea);
		}
	}
    // ----------------------------------------------------------------------
    // Returns true if the object object is visible excluding all animations.
    public bool IsVisibleInLayout {
        get {
            var parent= Parent;
            if(parent == null) return true;    
            if(parent.IsIconizedInLayout) return false;
            if(IsNode && parent.IsFoldedInLayout) return false;
            return parent.IsVisibleInLayout;            
        }
    }
    // ----------------------------------------------------------------------
    public bool IsVisibleOnDisplay {
        get {
            if(IsPort) {
                var parent= ParentNode;
                return parent.IsVisibleOnDisplay;
            }
            if(!IsAnimated) return IsVisibleInLayout;
            var area= Math3D.Area(DisplaySize);
            return Math3D.IsGreater(area, iCS_EditorConfig.kMinIconicArea);
        }
    }
    // ----------------------------------------------------------------------
    // Returns true if the display size is currently being animated.
    public bool IsDisplaySizeAnimated {
        get {
            return  myAnimatedDisplaySize.IsActive;
        }
    }
    // ----------------------------------------------------------------------
    // Returns true if the display position is currently being animated.
    public bool IsDisplayPositionAnimated {
        get {
            return  myAnimatedDisplayPosition.IsActive;            
        }
    }
    // ----------------------------------------------------------------------
    // Returns true if the display size or position are being animated.
    public bool IsAnimated {
        get {
            if(IsDisplaySizeAnimated) return true;
            return IsDisplayPositionAnimated;
        }
    }

    // ======================================================================
    // Display State Change
    // ----------------------------------------------------------------------
    // FIXME: Ports are not properly animated on unfold/fold/iconize.    
    // IMPROVE: Should avoid digging through all objects to animate them on unfold/fold/iconize.
    public void Iconize() {
        if(DisplayOption == iCS_DisplayOptionEnum.Iconized) return;
		SetStartValueForDisplayRectAnimation();
		var timer= BuildStandardAnimationTimer();
		{
            // Prepare to animate child nodes if node was unfolded.
            bool animateChildren= DisplayOption == iCS_DisplayOptionEnum.Unfolded;
            if(animateChildren) {
      		    ForEachChildRecursiveDepthFirst(
      			    c=> {
      				    if(c.IsNode && c.IsVisibleInLayout) {
      					    c.SetStartValueForDisplayRectAnimation();
      				    }
      			    }
      		    );                
            }
            // Set the node has iconized.
	        DisplayOption= iCS_DisplayOptionEnum.Iconized;
	        LayoutNode();
	        LayoutParentNodesUntilTop();
            // Animate child nodes if node was unfolded.
	        if(animateChildren) {
    			ForEachChildRecursiveDepthFirst(
    				c=> {
    					if(c.IsNode && !c.IsVisibleInLayout) {
    						c.StartDisplayRectAnimation(timer);
    					}
    				}
    			);	            
	        }				
		}
		StartDisplayRectAnimation(timer);
        IsDirty= true;
    }
    // ----------------------------------------------------------------------    
    public void Fold() {
        if(DisplayOption == iCS_DisplayOptionEnum.Folded) return;
		SetStartValueForDisplayRectAnimation();
		var timer= BuildStandardAnimationTimer();
		{
            // Prepare to animate child nodes if node was unfolded.
            bool animateChildren= DisplayOption == iCS_DisplayOptionEnum.Unfolded;
            if(animateChildren) {
      		    ForEachChildRecursiveDepthFirst(
      			    c=> {
      				    if(c.IsNode && c.IsVisibleInLayout) {
      					    c.SetStartValueForDisplayRectAnimation();
      				    }
      			    }
      		    );                
            }
            // Set the node has folded.
	        DisplayOption= iCS_DisplayOptionEnum.Folded;
	        LayoutNode();
	        LayoutParentNodesUntilTop();
            // Animate child nodes if node was unfolded.
	        if(animateChildren) {
    			ForEachChildRecursiveDepthFirst(
    				c=> {
    					if(c.IsNode && !c.IsVisibleInLayout) {
    						c.StartDisplayRectAnimation(timer);
    					}
    				}
    			);	            
	        }	
		}
		StartDisplayRectAnimation(timer);
        IsDirty= true;
    }
    // ----------------------------------------------------------------------    
    public void Unfold() {
        if(DisplayOption == iCS_DisplayOptionEnum.Unfolded) return;
		SetStartValueForDisplayRectAnimation();
		var timer= BuildStandardAnimationTimer();
		{
            // Prepare to animate child nodes that may become visible when unfolding.
  		    ForEachChildRecursiveDepthFirst(
  			    c=> {
  				    if(c.IsNode && !c.IsVisibleInLayout) {
  					    c.SetStartValueForDisplayRectAnimation();
  				    }
  			    }
  		    );
            // Set the node has unfolded.
	        DisplayOption= iCS_DisplayOptionEnum.Unfolded;
	        LayoutNode();
	        LayoutParentNodesUntilTop();
            // Animate child nodes that become visible by unfolding.
			ForEachChildRecursiveDepthFirst(
				c=> {
					if(c.IsNode && c.IsVisibleInLayout) {
						c.StartDisplayRectAnimation(timer);
					}
				}
			);
		}
		StartDisplayRectAnimation(timer);
        IsDirty= true;
    }

}
