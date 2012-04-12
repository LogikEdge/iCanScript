using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public class DSVerticalLayoutView : DSView {
//    // ======================================================================
//    // Fields
//    // ----------------------------------------------------------------------
//	List<DSView>    mySubviews= null;
//	
//    // ======================================================================
//    // Properties
//    // ----------------------------------------------------------------------
//	
//    // =================================================================================
//    // Constants
//    // ---------------------------------------------------------------------------------
//    
//    // ======================================================================
//    // Initialization
//    // ----------------------------------------------------------------------
//    public DSVerticalLayoutView(RectOffset margins, bool shouldDisplayFrame= true)
//    : base(margins, shouldDisplayFrame) {
//        mySubviews= new List<DSView>();
//    }
//    
//    // ======================================================================
//    // Display
//    // ----------------------------------------------------------------------
//    public override void Display(DSView parent, Rect frameArea) {
//        // Duisplay bounding box and title.
//        base.Display();
//        ForEachSubview(
//            sv=> {
//                sv.Display();
//            }
//        );
//    }
//    // ----------------------------------------------------------------------
//	void Relayout() {
//        // Determine minimum & maximum height.
//		float totalMinHeight= 0;
//		float totalMaxHeight= 0;
//		ForEachSubview(
//            sv=> {
//    			totalMinHeight+= sv.MinimumFrameSize.y;
//                totalMaxHeight+= sv.FullFrameSize.y;                
//            }
//		);
//		float totalDeltaHeight= totalMaxHeight-totalMinHeight;
//		// Compute frame size for each subview.
//        float remainingHeight= BodyArea.height-totalMinHeight;
//        if(remainingHeight < 0) remainingHeight= 0;
//        float y= BodyArea.y;
//        ForEachSubview(
//            sv=> {
//                var minSize= sv.MinimumFrameSize;
//                var maxSize= sv.FullFrameSize;
//    		    float delta= remainingHeight*(maxSize.y-minSize.y)/totalDeltaHeight;
//    		    float height= minSize.y+delta;
//    		    if(height > maxSize.y) height= maxSize.y;
//    		    if(sv.DisplayRatio.y != 0 && height > BodyArea.height*sv.DisplayRatio.y) height= BodyArea.height*sv.DisplayRatio.y;
//    		    if(height < minSize.y) height= minSize.y;
//    		    remainingHeight-= height- minSize.y;
//    		    totalDeltaHeight-= maxSize.y-minSize.y;
//    		    sv.FrameArea= new Rect(BodyArea.x, y, BodyArea.width, height);
//    		    y+= height;                
//            }
//        );
//	}
//	
//    // ======================================================================
//    // View dimension change notification.
//    // ----------------------------------------------------------------------
//    public override void OnViewChange(DSView parent, Rect displayArea) {
//		base.OnViewAreaChange(parent, displayArea);
//		Relayout();
//	}
//	
//	// ======================================================================
//    // Subview management
//    // ----------------------------------------------------------------------
//    public void AddSubview(DSView subview) {
//        mySubviews.Add(subview);
//    }
//    public override bool RemoveSubview(DSView subview) {
//		return mySubviews.Remove(subview);
//    }
//
//    // ----------------------------------------------------------------------
//    protected override Vector2 GetMinimumFrameSize() {
//		Vector2 size= Vector2.zero;
//		ForEachSubview(
//			sv=> {
//				var svSize= sv.MinimumFrameSize;
//				if(svSize.x > size.x) size.x= svSize.x;
//				size.y+= svSize.y;
//			}
//		);
//		return size+MarginsSize;
//	}
//    // ----------------------------------------------------------------------
//    protected Vector2 GetFullFrameSize() {
//		Vector2 size= Vector2.zero;
//		ForEachSubview(
//			sv=> {
//				var svSize= sv.FullFrameSize;
//				if(svSize.x > size.x) size.x= svSize.x;
//				size.y+= svSize.y;
//			}
//		);
//		return size+MarginsSize;
//	}
//
//}
//