//#define SHOW_POSITION
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

// ==========================================================================
// Node utilities.
// ==========================================================================
public partial class iCS_Graphics {
    // ----------------------------------------------------------------------
    bool ShouldDisplayNodeName(iCS_EditorObject node) {
        if(!ShouldShowTitle()) return false;
        if(!node.IsNode) return false;
        if(!node.IsVisibleOnDisplay) return false;
        return true;
    }
    // ----------------------------------------------------------------------
    // Returns the scaled node name size.
	string GetNodeName(iCS_EditorObject node) {
#if SHOW_POSITION
        return node.NodeTitle+" GP:"+node.GlobalPosition+" CO:"+node.CollisionOffset+" WO:"+node.WrappingOffset;
#else
        return node.NodeTitle;
#endif
	}
    // ----------------------------------------------------------------------
    // Returns the scaled node name size.
    Vector2 GetNodeNameSize(iCS_EditorObject node) {
        string nodeName= GetNodeName(node);
        GUIContent content= new GUIContent(nodeName);
        return node.IsIconizedOnDisplay ? LabelStyle.CalcSize(content) : TitleStyle.CalcSize(content);
    }
    // ----------------------------------------------------------------------
    // Returns the non-scaled x,y with the scaled size.
    Rect GetNodeNamePosition(iCS_EditorObject node) {
        Vector2 size= GetNodeNameSize(node);
        Rect pos= node.AnimatedRect;
        float x= 0.5f*(pos.x+pos.xMax-size.x/Scale);
        float y= pos.y;
        if(node.IsIconizedOnDisplay) {
            y-= 5f+size.y/Scale;
        } else {
			y+= 0.9f*kNodeCornerRadius-0.5f*size.y/Scale;
        }
        return new Rect(x, y, size.x, size.y);
    }
    // ----------------------------------------------------------------------
    // Returns the scaled x,y,size.
    public Rect GetNodeNameGUIPosition(iCS_EditorObject node) {
        Rect graphRect= GetNodeNamePosition(node);
        var guiPos= TranslateAndScale(Math3D.ToVector2(graphRect));
        return new Rect(guiPos.x, guiPos.y, graphRect.width, graphRect.height);	    
    }
    // ----------------------------------------------------------------------
    // Returns the tooltip for the given node.
	public static string GetNodeTooltip(iCS_EditorObject node) {
		string tooltip= "Name: "+(node.RawName ?? "")+"\n";
		// Type information
		Type runtimeType= node.RuntimeType;
		if(runtimeType != null) tooltip+= "Type: "+iCS_Types.TypeName(runtimeType)+"\n";
		// Number of direct children
		int nbOfChildren= node.NbOfChildNodes;
		tooltip+= "Child nodes: "+nbOfChildren+"\n";
		// User defined tooltip
		if(iCS_Strings.IsNotEmpty(node.Tooltip)) tooltip+= node.Tooltip;
		return tooltip;
	}
    // ----------------------------------------------------------------------
    public static bool IsActiveState(iCS_EditorObject state) {
        if(!state.IsState) return false;
        if(!Application.isPlaying) return false;
        var stateChart= state.GetParentStateChart();
        if(stateChart == null) return false;
        var runtimeNodes= state.IStorage.VisualScript.RuntimeNodes;
        var rtStateChart= runtimeNodes[stateChart.InstanceId] as iCS_StateChart;
        var rtState     = runtimeNodes[state.InstanceId] as iCS_State;
        return rtStateChart.IsActiveState(rtState);
    }
}
