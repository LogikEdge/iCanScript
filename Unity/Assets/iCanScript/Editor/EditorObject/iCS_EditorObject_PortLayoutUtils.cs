using UnityEngine;
using System.Collections;

public partial class iCS_EditorObject {
    // ======================================================================
    // Ports layout helpers.
    // ----------------------------------------------------------------------
    // Returns the available height to layout ports on the vertical edge.
    public float AvailableHeightForPorts {
        get {
            if(!IsVisibleOnDisplay) return 0f;
			if(IsTransitionModule && IsIconizedOnDisplay) return 0f;
            return VerticalPortsBottom-VerticalPortsTop;
        }
    }
    // ----------------------------------------------------------------------
    // Returns the available width to layout ports on the horizontal edge.
    public float AvailableWidthForPorts {
        get {
            if(!IsVisibleOnDisplay) return 0f;
			if(IsTransitionModule && IsIconizedOnDisplay) return 0f;
            return HorizontalPortsRight-HorizontalPortsLeft;
        }
    }

    // ----------------------------------------------------------------------
    // Returns the top most coordinate for a port on the vertical edge.
    public float VerticalPortsTop {
        get {
            if(!IsVisibleOnDisplay) return 0f;
            if(IsIconizedOnDisplay) {
				return IsTransitionModule ? 0f : -0.25f*AnimatedSize.y;
			}
            return NodeTitleHeight+0.5f*(iCS_EditorConfig.MinimumPortSeparation-AnimatedSize.y);
        }
    }
    // ----------------------------------------------------------------------
    // Returns the bottom most coordinate for a port on the vertical edge.
    public float VerticalPortsBottom {
        get {
            if(!IsVisibleOnDisplay) return 0f;
            if(IsIconizedOnDisplay) {
				return IsTransitionModule ? 0f : 0.25f*AnimatedSize.y;
			}
            return 0.5f*(AnimatedSize.y-iCS_EditorConfig.MinimumPortSeparation);
        }
    }
    // ----------------------------------------------------------------------
    // Returns the left most coordinate for a port on the horizontal edge.
    public float HorizontalPortsLeft {
        get {
            if(!IsVisibleOnDisplay) return 0f;
            if(IsIconizedOnDisplay) {
				return IsTransitionModule ? 0f : -0.25f*AnimatedSize.x;
			}
            return 0.5f*(iCS_EditorConfig.MinimumPortSeparation-AnimatedSize.x);
        }
    }
    // ----------------------------------------------------------------------
    // Returns the left most coordinate for a port on the horizontal edge.
    public float HorizontalPortsRight {
        get {
            if(!IsVisibleOnDisplay) return 0f;
            if(IsIconizedOnDisplay) {
				return IsTransitionModule ? 0f : 0.25f*AnimatedSize.x;
			}
            return 0.5f*(AnimatedSize.x-iCS_EditorConfig.MinimumPortSeparation);
        }
    }    
    // ----------------------------------------------------------------------
    // Returns the minimium height needed for the left / right ports.
    public float MinimumHeightForPorts {
        get {
            int nbOfPorts= Mathf.Max(NbOfLeftPorts, NbOfRightPorts);
            return nbOfPorts*iCS_EditorConfig.MinimumPortSeparation;                                            
        }
    }
    // ----------------------------------------------------------------------
    // Returns the minimum width needed for the top / bottom ports.
    public float MinimumWidthForPorts {
        get {
            int nbOfPorts= Mathf.Max(NbOfTopPorts, NbOfBottomPorts);
            return nbOfPorts*iCS_EditorConfig.MinimumPortSeparation;                                            
        }
    }
    
}
