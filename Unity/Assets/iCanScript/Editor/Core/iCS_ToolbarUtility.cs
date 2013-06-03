using UnityEngine;
using UnityEditor;
using System.Collections;

public static class iCS_ToolbarUtility {
    // ======================================================================
    // Toolbar utilities.
	// ----------------------------------------------------------------------
    public static Rect BuildToolbar(float width, float yOffset= 0) {
        Rect toolbarRect= new Rect(0,yOffset,width,GetHeight());
        Rect r= toolbarRect;
        r.height+= 20f;
		GUI.Box(r, "", EditorStyles.toolbar);
        return toolbarRect;
    }
	// ----------------------------------------------------------------------
    public static float GetHeight() {
		return iCS_EditorUtility.GetGUIStyleHeight(EditorStyles.toolbar);        
    }	// ----------------------------------------------------------------------
	public static Rect ReserveArea(ref Rect r, float width, float leftMargin, float rightMargin, bool isRightJustified) {
        // Validate that we have the space asked.
        float totalSize= width+leftMargin+rightMargin;
        if(totalSize > r.width) {
            // We cannot allocate the asked size, so lets reduce the width.
            width= r.width-leftMargin-rightMargin;
            if(width <= 0) return new Rect(r.x,r.y,0,r.height);
        }
        Rect result= new Rect(0,0,0,0);
        if(isRightJustified) {
    		result= new Rect(r.xMax-width-rightMargin, r.y, width, r.height);
            
        } else {
    		result= new Rect(r.x+leftMargin, r.y, width, r.height);
    		r.x+= totalSize;
        }
		r.width-= totalSize;            
		return result;
	} 

	// ----------------------------------------------------------------------
    public static void MiniLabel(ref Rect toolbarRect, string label, float leftMargin, float rightMargin, bool isRightJustified= false) {
        MiniLabel(ref toolbarRect, new GUIContent(label), leftMargin, rightMargin, isRightJustified);
	}
	// ----------------------------------------------------------------------
    public static void MiniLabel(ref Rect toolbarRect, GUIContent content, float leftMargin, float rightMargin, bool isRightJustified= false) {
		var contentSize= EditorStyles.miniLabel.CalcSize(content);
		Rect r= ReserveArea(ref toolbarRect, contentSize.x, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return;
        float offset= 0.5f*(r.height-contentSize.y);
        r.y+= offset;
        r.height-= offset;
		GUI.Label(r, content, EditorStyles.miniLabel);        
    }
	// ----------------------------------------------------------------------
    public static void MiniLabel(ref Rect toolbarRect, float width, string label, float leftMargin, float rightMargin, bool isRightJustified= false) {
        MiniLabel(ref toolbarRect, width, new GUIContent(label), leftMargin, rightMargin, isRightJustified);
    }
	// ----------------------------------------------------------------------
    public static void MiniLabel(ref Rect toolbarRect, float width, GUIContent content, float leftMargin, float rightMargin, bool isRightJustified= false) {
		var contentSize= EditorStyles.miniLabel.CalcSize(content);
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return;
        float offset= 0.5f*(r.height-contentSize.y);
        r.y+= offset;
        r.height-= offset;
		GUI.Label(r, content, EditorStyles.miniLabel);        
    }
	// ----------------------------------------------------------------------
    public static float Slider(ref Rect toolbarRect, float width, float value, float leftValue, float rightValue, float rightMargin, float leftMargin, bool isRightJustified= false) {
		var contentSize= GUI.skin.horizontalSlider.CalcSize(new GUIContent());
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return value;
        float offset= 0.5f*(r.height-contentSize.y);
        r.y+= offset;
        r.height-= offset;
        return GUI.HorizontalSlider(r, value, leftValue, rightValue);
    }
	// ----------------------------------------------------------------------
    public static string Text(ref Rect toolbarRect, float width, string value, float leftMargin, float rightMargin, bool isRightJustified= false) {
        GUIContent content= new GUIContent(value);
		var contentSize= EditorStyles.toolbarTextField.CalcSize(content);
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return value;
        float offset= 0.5f*(r.height-contentSize.y);
        r.y+= offset;
        r.height-= offset;
        return GUI.TextField(r, value, EditorStyles.toolbarTextField);
    }
	// ----------------------------------------------------------------------
	// FIXME: CenterLabel: left margin not functional
    public static void CenteredTitle(ref Rect toolbarRect, string title) {
        GUIContent content= new GUIContent(title);
        var style= EditorStyles.boldLabel;
		var contentSize= style.CalcSize(content);
		float w= contentSize.x;
		float dx= 0.5f*(toolbarRect.width-w);
		if(dx < 0) {
			dx= 0;
			w= toolbarRect.width;
		}
        float offset= 0.5f*(toolbarRect.height-contentSize.y);
        toolbarRect.y+= offset;
        toolbarRect.height-= offset;
		toolbarRect.x+= dx;
		toolbarRect.width= w;
        GUI.Label(toolbarRect, content, style);
		toolbarRect.width= 0;
    }
	// ----------------------------------------------------------------------
    public static int Buttons(ref Rect toolbarRect, float width, int value, string[] options, float leftMargin, float rightMargin, bool isRightJustified= false) {
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return value;
        int newValue= GUI.Toolbar(r, value, options, EditorStyles.toolbarButton);
		return newValue;
    }
	// ----------------------------------------------------------------------
    public static string Search(ref Rect toolbarRect, float width, string value, float leftMargin, float rightMargin, bool isRightJustified= false) {
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        float iconSize= GetHeight();
        if(r.width < iconSize+1f) return value;
        r.y+= 2f;
        r.width-= iconSize;
        Texture2D cancelIcon= null;
        if(iCS_TextureCache.GetTexture(iCS_EditorStrings.CancelIcon, out cancelIcon)) {
            GUI.DrawTexture(new Rect(r.xMax, r.y, iconSize, iconSize), cancelIcon);
        } else {
            Debug.LogWarning("iCanScript: Cannot find cancel Icon in resource folder !!!");
        }
        return GUI.TextField(r, value, EditorStyles.toolbarTextField);        
    }
	// ----------------------------------------------------------------------
    public static int Popup(ref Rect toolbarRect, float width, string label, int index, string[] options, float leftMargin, float rightMargin, bool isRightJustified= false) {
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return index;
        return EditorGUI.Popup(r, label, index, options, EditorStyles.toolbarDropDown);
    }
	// ----------------------------------------------------------------------
    public static int Popup(ref Rect toolbarRect, float width, GUIContent content, int index, GUIContent[] options, float leftMargin, float rightMargin, bool isRightJustified= false) {
		Rect r= ReserveArea(ref toolbarRect, width, leftMargin, rightMargin, isRightJustified);		
        if(r.width < 1f) return index;
        return EditorGUI.Popup(r, content, index, options, EditorStyles.toolbarDropDown);
    }
	// ----------------------------------------------------------------------
    public static bool Toggle(ref Rect toolbarRect, bool value, float leftMargin, float rightMargin, bool isRightJustified= false) {
        GUIContent content= new GUIContent();
        var contentSize= GUI.skin.toggle.CalcSize(content);
		Rect r= ReserveArea(ref toolbarRect, contentSize.x, leftMargin, rightMargin, isRightJustified);
        if(r.width < 1f) return value;
        float offset= 0.5f*(r.height-contentSize.y);
        r.y+= offset;
        r.height-= offset;
		return GUI.Toggle(r, value, "");
    }
	// ----------------------------------------------------------------------
    public static void Separator(ref Rect toolbarRect, bool isRightJustified= false) {
        Rect r= ReserveArea(ref toolbarRect, 2, 0, 0, isRightJustified);
        GUI.Box(r, "", EditorStyles.toolbarButton);
    }
}
