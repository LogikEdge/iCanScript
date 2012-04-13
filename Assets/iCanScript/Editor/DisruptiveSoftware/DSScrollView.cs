using UnityEngine;
using System;
using System.Collections;

public class DSScrollView : DSView {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    Vector2                         myScrollPosition          = Vector2.zero;
    Vector2                         myContentSize             = Vector2.zero;
    DSCellView                      myMainView                = null;
 	Action<DSScrollView,Rect>       myDisplayDelegate         = null;
	Func<DSScrollView,Rect,Vector2> myGetSizeToDisplayDelegate= null;
   
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
	Rect ContentArea { get { return new Rect(0,0,myContentSize.x,myContentSize.y); }}
	public Action<DSScrollView,Rect> DisplayDelegate {
	    get { return myDisplayDelegate; }
	    set { myDisplayDelegate= value; }
	}
	public Func<DSScrollView,Rect,Vector2> GetSizeToDisplayDelegate {
	    get { return myGetSizeToDisplayDelegate; }
	    set { myGetSizeToDisplayDelegate= value; }
	}
	
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public DSScrollView(RectOffset margins, bool shouldDisplayFrame,
                        Action<DSScrollView,Rect> displayDelegate,
                        Func<DSScrollView,Rect,Vector2> getSizeToDisplayDelegate) {
        myMainView= new DSCellView(margins, shouldDisplayFrame, MainViewDisplay, MainViewGetSizeToDisplay);
        myDisplayDelegate= displayDelegate;
        myGetSizeToDisplayDelegate= getSizeToDisplayDelegate;
    }

    // ======================================================================
    // DSView implementation.
    // ----------------------------------------------------------------------
    public override void Display(Rect frameArea) {
        myMainView.Display(frameArea);
    }
    public override Vector2 GetSizeToDisplay(Rect frameArea) {
        return myMainView.GetSizeToDisplay(frameArea);
    }
    public override AnchorEnum GetAnchor() {
        return myMainView.Anchor;
    }
    public override void SetAnchor(AnchorEnum anchor) {
        myMainView.Anchor= anchor;
    }

    // ======================================================================
    // MainView implementation.
    // ----------------------------------------------------------------------
    void MainViewDisplay(DSCellView view, Rect displayArea) {
        myScrollPosition= GUI.BeginScrollView(displayArea, myScrollPosition, ContentArea, false, false);
            InvokeDisplayDelegate(ContentArea);
        GUI.EndScrollView();
    }
    Vector2 MainViewGetSizeToDisplay(DSCellView view, Rect displayArea) {
		myContentSize= InvokeGetSizeToDisplayDelegate(displayArea);
        // Add scroller if the needed display size exceeds the display area.
		var contentSize= myContentSize;        
        if(displayArea.width < myContentSize.x) contentSize.y+= kScrollerSize;
        if(displayArea.height < myContentSize.y) contentSize.x+= kScrollerSize;
        Debug.Log("ContentSize= "+myContentSize+" DisplayArea= "+displayArea+" ResultingSize= "+contentSize);
        return contentSize;
    }
    
    // ======================================================================
    // Delegates.
    // ----------------------------------------------------------------------
    protected void InvokeDisplayDelegate(Rect displayArea) {
    	if(myDisplayDelegate != null) myDisplayDelegate(this, displayArea);        
    }
    protected Vector2 InvokeGetSizeToDisplayDelegate(Rect displayArea) {
    	return myGetSizeToDisplayDelegate != null ? myGetSizeToDisplayDelegate(this, displayArea) : Vector2.zero;        
    }
}
