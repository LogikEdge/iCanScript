using UnityEngine;
using System;
using System.Collections;

public class DSCellView : DSView {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    Action<Rect>    myDrawCellAction= null;
    Func<Vector2>   myGetDisplaySize= null;
    Vector2         myDataSize      = Vector2.zero;
        
    // ======================================================================
    // Propreties
    // ----------------------------------------------------------------------
    public Action<Rect> DrawCellAction {
        get { return myDrawCellAction; }
        set { myDrawCellAction= value; }
    }
    public Func<Vector2> GetDisplaySize {
        get { return myGetDisplaySize; }
        set { myGetDisplaySize= value; ReloadData(); }
    }

    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public DSCellView(Func<Vector2> getDisplaySizeFnc, Action<Rect> drawCellAction,
                      RectOffset margins, bool shouldDisplayFrame= false)
     : base(margins, shouldDisplayFrame) {
        myDrawCellAction= drawCellAction;
        myGetDisplaySize= getDisplaySizeFnc;
        ReloadData();
    }
    
    // ======================================================================
    // Display
    // ----------------------------------------------------------------------
    protected override void Display() {
        if(myDrawCellAction != null) myDrawCellAction(DisplayArea);
    }
    protected override void ViewAreaDidChange() {
        base.ViewAreaDidChange();
        ReloadData();
    }
    protected override Vector2 GetMinimumFrameSize() {
        return base.GetMinimumFrameSize() + myDataSize;
    }

    public void ReloadData() {
        if(myGetDisplaySize != null) myDataSize= myGetDisplaySize();        
    }

}
