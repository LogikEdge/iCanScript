using UnityEngine;
using UnityEditor;
using System.Collections;
using Prefs= iCS_PreferencesController;
using iCanScriptEditor;

/*
    TODO: Should show frameId in header bar.
*/
public partial class iCS_VisualEditor : iCS_EditorBase {
    // =======================================================================
    // Toolbar Constants
    // -----------------------------------------------------------------------
    const float kSliderSize= 60f;
    
    // -----------------------------------------------------------------------
	void Toolbar() {
		// No toolbar if editor snapshot without background requested.
		if(iCS_DevToolsConfig.IsFrameWithoutBackground) return;
		
		// Build standard toolbar at top of editor window.
		Rect r= iCS_ToolbarUtility.BuildToolbar(position.width, 0f);

		// Insert an initial spacer.
		float spacer= 8f;
//		r.x+= spacer;
//		r.width-= spacer;

		// --------------
		// LEFT TOOLBAR
        // Navigation History
        var backwardNavigationIcon= iCS_BuiltinTextures.BackwardNavigationHistoryIcon();
        var forwardNavigationIcon= iCS_BuiltinTextures.ForwardNavigationHistoryIcon();
        var hasBackwardHistory= IStorage.HasBackwardNavigationHistory;
        float width= backwardNavigationIcon.width+spacer;
        if(iCS_ToolbarUtility.Button(ref r, width, hasBackwardHistory, backwardNavigationIcon, 0, 0)) {
            if(hasBackwardHistory) {
                iCS_UserCommands.ReloadFromBackwardNavigationHistory(IStorage);
            }            
        }
        var hasForwardHistory= IStorage.HasForwardNavigationHistory;
        width= forwardNavigationIcon.width+spacer;
        if(iCS_ToolbarUtility.Button(ref r, width, hasForwardHistory, forwardNavigationIcon, 0, 0)) {
            if(hasForwardHistory) {
                iCS_UserCommands.ReloadFromForwardNavigationHistory(IStorage);
            }            
        }
		// Show Display Root Node.
		GUI.changed= false;
		IStorage.ShowDisplayRootNode= iCS_ToolbarUtility.Toggle(ref r, IStorage.ShowDisplayRootNode, spacer, spacer);
        iCS_ToolbarUtility.MiniLabel(ref r, "Show Root Node", 0,0);
        iCS_ToolbarUtility.Separator(ref r);

        // Debug Controls
        if(VisualScript != null /*&& Application.isPlaying*/) {
    		// Show Runtime frame id.
            bool showFrameId= Prefs.ShowRuntimeFrameId;
            bool newShowFrameId= iCS_ToolbarUtility.Toggle(ref r, showFrameId, 0, 0);
            if(newShowFrameId != showFrameId) {
                Prefs.ShowRuntimeFrameId= newShowFrameId;
            }
            iCS_ToolbarUtility.MiniLabel(ref r, "Frame Id", 0,0);
            iCS_ToolbarUtility.Separator(ref r);
    		
            // Enable Traces.
            bool enableTrace= VisualScript.IsTraceEnabled;
            bool newEnableTrace= iCS_ToolbarUtility.Toggle(ref r, enableTrace, spacer, spacer);
            if(newEnableTrace != enableTrace) {
                VisualScript.IsTraceEnabled= newEnableTrace;
            }            
            iCS_ToolbarUtility.MiniLabel(ref r, "Trace", 0,0);
            iCS_ToolbarUtility.Separator(ref r);
        
            // Show Runtime values.
            bool showRuntime= Prefs.ShowRuntimePortValue;
            bool newShowRuntime= iCS_ToolbarUtility.Toggle(ref r, showRuntime, spacer, spacer);
            if(newShowRuntime != showRuntime) {
                Prefs.ShowRuntimePortValue= newShowRuntime;
            }
            iCS_ToolbarUtility.MiniLabel(ref r, "Runtime Values", 0,0);
            float refreshSpeed= Mathf.Sqrt(Prefs.PortValueRefreshPeriod);
            float newRefreshSpeed= iCS_ToolbarUtility.Slider(ref r, kSliderSize, refreshSpeed, 0.3162f, 1.414f, spacer, spacer);
            if(newRefreshSpeed != refreshSpeed) {
                Prefs.PortValueRefreshPeriod= newRefreshSpeed*newRefreshSpeed;
            }
            iCS_ToolbarUtility.Separator(ref r);
		    // Refresh Preferences window
		    if(GUI.changed) {
		    	iCS_EditorController.RepaintPreferencesEditor();
		    }
        }
		
		// --------------
		// RIGHT TOOLBAR
		// Show zoom control at the end of the toolbar.
        float newScale= iCS_ToolbarUtility.Slider(ref r, kSliderSize, Scale, 2f, 0.15f, spacer, spacer, true);
        iCS_ToolbarUtility.MiniLabel(ref r, "Zoom", 10f, 0, true);
		if(Math3D.IsNotEqual(newScale, Scale)) {
            Vector2 pivot= ViewportToGraph(ViewportCenter);
            CenterAtWithScale(pivot, newScale);
		}
		iCS_ToolbarUtility.Separator(ref r, true);

//		// Show current bookmark.
//		string bookmarkString= "Bookmark: ";
//		if(myBookmark == null) {
//		    bookmarkString+= "(empty)";
//		} else {
//		    bookmarkString+= myBookmark.Name;
//		}
//		iCS_ToolbarUtility.MiniLabel(ref r, 150f, bookmarkString, spacer, 0, true);
//        iCS_ToolbarUtility.Separator(ref r, true);

		// --------------
		// CENTER TOOLBAR
        // Show game object name in middle of toolbar.
		var name= DisplayRoot.FullName;
		if(Application.isPlaying) {
			var visualScript= IStorage.iCSMonoBehaviour as iCS_VisualScriptImp;
			if(visualScript != null) {
				name+= " (id= "+visualScript.UpdateFrameId;
				if(Math3D.IsNotZero(Time.smoothDeltaTime)) {
					int frameRate= (int)(1f/Time.smoothDeltaTime);
					name+="; fr= "+frameRate;
				}
				name+=")";				
			}
		}
		iCS_ToolbarUtility.CenteredTitle(ref r, name);

        // Trial information.
        ShowTrialInformation(ref r);

		// Show scroll position
		var scrollPositionAsStr= ScrollPosition.ToString();
		var scrollPositionAsGUIContent= new GUIContent(scrollPositionAsStr);
		var scrollPositionSize= EditorStyles.label.CalcSize(scrollPositionAsGUIContent);
		r= new Rect(position.width-scrollPositionSize.x, position.height-scrollPositionSize.y,
			        scrollPositionSize.x, scrollPositionSize.y);
		GUI.Label(r, scrollPositionAsGUIContent);
	}
    // -----------------------------------------------------------------------
    void ShowTrialInformation(ref Rect r) {
        if(EditionController.IsCommunityEdition) {
            // -- Display trial remaining information --
            GUIStyle style= EditorStyles.textField;
            style.richText= true;
            style.fontStyle= FontStyle.Bold;
            style.fontSize= (int)(style.fontSize*1.1f);
            var nbOfVisualScriptRemaining= EditionController.CommunityVisualScriptsRemaining;
            var nbOfNodesRemaining= EditionController.CommunityNodesRemaining;
            var visualScriptsMessage= "Visual Scripts Remaining= "+nbOfVisualScriptRemaining;
            var nodesMessage="Nodes Remaining= "+nbOfNodesRemaining;
            var percentVisualScriptsRemaining= EditionController.CommunityPercentVisualScriptsRemaining;
            var percentNodesRemaining        = EditionController.CommunityPercentNodesRemaining;
            if(percentVisualScriptsRemaining <= 0.4f) {
                if(percentVisualScriptsRemaining <= 0.2f) {
                    visualScriptsMessage= "<color=red>"+visualScriptsMessage+"</color>";
                }
                else {
                    visualScriptsMessage= "<color=orange>"+visualScriptsMessage+"</color>";                    
                }
            }
            if(percentNodesRemaining <= 0.4f) {
                if(percentNodesRemaining <= 0.2f) {
                    nodesMessage= "<color=red>"+nodesMessage+"</color>";
                }
                else {
                    nodesMessage= "<color=orange>"+nodesMessage+"</color>";
                }
            }
            var trialMessage= "COMMUNITY EDITION ==> "+visualScriptsMessage+" / "+nodesMessage+" <==";
            var trialMessageSize= style.CalcSize(new GUIContent(trialMessage));
            var x= 0.5f*(position.width-trialMessageSize.x);
            GUI.Label(new Rect(x,r.yMax,trialMessageSize.x, trialMessageSize.y), trialMessage, style);

//            // -- Display Purchase information --
//            if(nbOfVisualScriptRemaining < 0 || nbOfNodesRemaining < 0) {
//                var toolbarHeight= iCS_ToolbarUtility.GetHeight();
//                var width = position.width;
//                var height= position.height-toolbarHeight;
//                var purchaseRect= new Rect(0.15f*width, toolbarHeight+0.15f*height, 0.7f*width, 0.7f*height);
//                GUI.Box(purchaseRect, "Please Purchase");
//            }
        }        
    }
}
