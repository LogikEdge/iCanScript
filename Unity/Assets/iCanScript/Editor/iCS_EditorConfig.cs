using UnityEngine;
using System.Collections;

public static class iCS_EditorConfig {
    // ======================================================================
    // Constants
    // ----------------------------------------------------------------------
    public const float kInitialScale    = 1f;
    public const float kIconicSize      = 32f;
    public const float kIconicArea      = kIconicSize*kIconicSize;
    public const float kMinIconicSize   = 12f;
    public const float kMinIconicArea   = kMinIconicSize*kMinIconicSize;
    public const float kNodeCornerRadius= 8f;
    public const float kNodeTitleHeight = 2f*kNodeCornerRadius;
    public const int   kLabelFontSize   = 11;
    public const int   kTitleFontSize   = 12;

    // ----------------------------------------------------------------------
	public const float NodeShadowSize= 5.0f;
	
    // ----------------------------------------------------------------------
    public const  float   PortRadius        = 5.55f;
    public const  float   PortDiameter      = 2.0f * PortRadius;
    public const  float   SelectedPortFactor= 1.67f;
    public static Vector2 PortSize;

    // ----------------------------------------------------------------------
    public const  float MarginSize = 15.0f;
    public const  float PaddingSize= 15.0f;

    // ----------------------------------------------------------------------
    public const float EditorWindowMarginSize = MarginSize;
    public const float EditorWindowPaddingSize= PaddingSize;
    public const float EditorWindowToolbarHeight= 16.0f;
    public const float EditorWindowMinX= EditorWindowMarginSize;
    public const float EditorWindowMinY= EditorWindowMarginSize + EditorWindowToolbarHeight;

    // ======================================================================
    public static GUIStyle NodeStyle        { get { return GUI.skin.button; }}
    public static GUIStyle PortLabelStyle   { get { return GUI.skin.label; }}
    
    // ======================================================================
	// Node title dimensions.
    public static Vector2 GetNodeTitleSize(string _label) {
        return NodeStyle.CalcSize(new GUIContent(_label));
    }
    public static float GetNodeTitleWidth(string _label) {
        return GetNodeTitleSize(_label).x;
    }
    public static float NodeTitleHeight {
        get {
            if(_NodeTitleHeight == 0f) {
				_NodeTitleHeight= GetNodeTitleSize("A").y;
			}
            return _NodeTitleHeight;
        }
    }
    static float _NodeTitleHeight= 0f;

    // ======================================================================
    // Port label dimensions.
    public static Vector2 GetPortLabelSize(string _label) {
        return PortLabelStyle.CalcSize(new GUIContent(_label));
    }
    public static float GetPortLabelWidth(string _label) {
        return GetPortLabelSize(_label).x;
    }
    public static float MinimumPortSeparation {
        get {
            if(_MinimumPortSeparation == 0f) {
				_MinimumPortSeparation= GetPortLabelSize("A").x;
			}
            return _MinimumPortSeparation;
        }
    }
    static float _MinimumPortSeparation= 0f;
 
    // ======================================================================
    static iCS_EditorConfig() {
        PortSize= new Vector2(PortDiameter, PortDiameter);
    }
}