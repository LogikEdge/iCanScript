using UnityEngine;
using UnityEditor;
using System.Collections;
using Prefs=iCS_PreferencesController;

public partial class iCS_VisualEditor : iCS_EditorBase {
	// ----------------------------------------------------------------------
    public void SmartFocusOn(iCS_EditorObject obj) {
        var focusNode= obj;
        // Focus on port parent.
        if(obj.IsPort) {
            focusNode= obj.ParentNode;
        }
        // Focus on parent:
        //   - if display option is iconized or folded;
        //   - node does not contain visual script
        if(focusNode.IsIconizedInLayout || focusNode.IsFoldedInLayout ||
           focusNode.IsInstanceNode || focusNode.IsKindOfFunction) {
            var parent= focusNode.ParentNode;
            if(parent != null) {
                focusNode= parent;
            }
        }
        iCS_EditorUtility.SafeCenterOn(focusNode, IStorage);                        
    }
	// ----------------------------------------------------------------------
    public void CenterOnRoot() {
        CenterOn(DisplayRoot);            
    }
	// ----------------------------------------------------------------------
    public void CenterAndScaleOnRoot() {
        CenterAndScaleOn(DisplayRoot);
    }
	// ----------------------------------------------------------------------
    public void CenterOnSelected() {
        CenterOn(SelectedObject ?? DisplayRoot);
    }
	// ----------------------------------------------------------------------
    public void CenterAndScaleOnSelected() {
        CenterAndScaleOn(SelectedObject ?? DisplayRoot);
    }
	// ----------------------------------------------------------------------
    public void CenterOn(iCS_EditorObject obj) {
        if(obj == null || IStorage == null) return;
        CenterAt(obj.LayoutPosition);
    }
	// ----------------------------------------------------------------------
    public void CenterAndScaleOn(iCS_EditorObject obj) {
        if(obj == null || IStorage == null) return;
        while(obj != null && !obj.IsVisibleInLayout) obj= obj.Parent;
        if(obj == null) return;
        var size= obj.LayoutSize;
        float newScale= 1.0f;
        if(obj.IsNode) {
            float widthScale= position.width/(1.1f*size.x);
            float heightScale= position.height/(1.1f*size.y);
            newScale= Mathf.Min(2.0f, Mathf.Min(widthScale, heightScale));
        }
        CenterAtWithScale(obj.LayoutPosition, newScale);
    }
	// ----------------------------------------------------------------------
    public void CenterAt(Vector2 point) {
        if(IStorage == null) return;
        Vector2 newScrollPosition= point-0.5f/Scale*new Vector2(position.width, position.height);
        float deltaTime= Prefs.AnimationTime;
        myAnimatedScrollPosition.Start(ScrollPosition, newScrollPosition, deltaTime, (start,end,ratio)=> Math3D.Lerp(start, end, ratio));
        ScrollPosition= newScrollPosition;
    }
	// ----------------------------------------------------------------------
    public void CenterAtWithScale(Vector2 point, float newScale) {
        if(IStorage == null) return;
        Vector2 newScrollPosition= point-0.5f/newScale*new Vector2(position.width, position.height);
        float deltaTime= Prefs.AnimationTime;
        myAnimatedScrollPosition.Start(ScrollPosition, newScrollPosition, deltaTime, (start,end,ratio)=> Math3D.Lerp(start, end, ratio));
        ScrollPosition= newScrollPosition;
        myAnimatedScale.Start(Scale, newScale, deltaTime, (start,end,ratio)=> Math3D.Lerp(start, end, ratio));
        Scale= newScale;
    }
	// ----------------------------------------------------------------------
    public void MakeVisibleInViewport(iCS_EditorObject obj) {
        var r= obj.LayoutRect;
        var clipArea= ClipingArea;
        var intersection= Math3D.Intersection(r, clipArea);
        if(Math3D.IsNotEqual(r, intersection)) {
            // By default, focus on parent
            var parent= obj.ParentNode;
            if(parent == null) {
                parent= obj;
            }
            CenterAndScaleOn(parent);
        }
    }
}
