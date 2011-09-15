using UnityEngine;
using System.Collections;

public class WD_ScrollView {
    // ----------------------------------------------------------------------
    public void Begin() {
        ScrollPosition= GUI.BeginScrollView(ScrollWindow, ScrollPosition, ScrollViewport);        
    }

    // ----------------------------------------------------------------------
    public void End() {
        GUI.EndScrollView();        
    }
    
    // ----------------------------------------------------------------------
    public void Update(Rect _windowPosition, Rect _rootNodePosition) {
	    // Adjust scroll window bounds.
        ScrollWindow= new Rect(0, WD_EditorConfig.EditorWindowToolbarHeight, _windowPosition.width, _windowPosition.height-WD_EditorConfig.EditorWindowToolbarHeight);

        // Update scroll viewport.
        Rect graphRect= new Rect(_rootNodePosition.x - WD_EditorConfig.GutterSize,
                                 _rootNodePosition.y - WD_EditorConfig.GutterSize,
                                 _rootNodePosition.width + 2*WD_EditorConfig.GutterSize,
                                 _rootNodePosition.height + 2*WD_EditorConfig.GutterSize);

        // Adjust top/left corner.
        if(ScrollViewport.x > graphRect.x) {
            float dx= ScrollViewport.x-graphRect.x;
            ScrollViewport.x= graphRect.x;
            ScrollPosition.x+= dx;
            ScrollViewport.width+= dx;
        }
        if(ScrollViewport.y > graphRect.y) {
            float dy= ScrollViewport.y-graphRect.y;
            ScrollViewport.y= graphRect.y;
            ScrollPosition.y+= dy;
            ScrollViewport.height+= dy;
        }
		if(ScrollViewport.xMax < graphRect.xMax) {
            ScrollViewport.width+= graphRect.xMax - ScrollViewport.xMax;
        }
        if(ScrollViewport.yMax < graphRect.yMax) {
            ScrollViewport.height+= graphRect.yMax - ScrollViewport.yMax;
        }
        
        // Clip viewport if it is larger then graph & window.
        if(!MathfExt.IsZero(ScrollPosition.x) && MathfExt.IsSmaller(ScrollViewport.x+ScrollPosition.x, graphRect.x)) {
            ScrollViewport.x+= ScrollPosition.x;
            ScrollViewport.width-= ScrollPosition.x;
            ScrollPosition.x= 0;
        }
        if(!MathfExt.IsZero(ScrollPosition.y) && MathfExt.IsSmaller(ScrollViewport.y+ScrollPosition.y, graphRect.y)) {
            ScrollViewport.y+= ScrollPosition.y;
            ScrollViewport.height-= ScrollPosition.y;
            ScrollPosition.y= 0;
        }
        if(MathfExt.IsGreater(ScrollViewport.xMax, graphRect.xMax)) {
            ScrollViewport.width-= ScrollViewport.xMax-graphRect.xMax;
        }
        if(MathfExt.IsGreater(ScrollViewport.yMax, graphRect.yMax)) {
            ScrollViewport.height-= ScrollViewport.yMax-graphRect.yMax;
        }
        
        // Adjust viewport width/height.
        if(MathfExt.IsSmaller(ScrollViewport.width, ScrollPosition.x+ScrollWindow.width)) {
            ScrollViewport.width= ScrollPosition.x+ScrollWindow.width;            
        }
        if(MathfExt.IsSmaller(ScrollViewport.height, ScrollPosition.y+ScrollWindow.height)) {
            ScrollViewport.height= ScrollPosition.y+ScrollWindow.height;                    
        }
    }
    
    // ----------------------------------------------------------------------
    // Convert's screen to graph coordinates.
    public Vector2 ScreenToGraph(Vector2 _v) {
        Vector2 viewportPosition= new Vector2(ScrollViewport.x, ScrollViewport.y);
        Vector2 ScrollWindowPosition= new Vector2(ScrollWindow.x, ScrollWindow.y); 
        return _v + viewportPosition + ScrollPosition - ScrollWindowPosition;
    }


    // ======================================================================
    // PROPERTIES
    // ----------------------------------------------------------------------
    Rect            ScrollWindow    = new Rect(0,0,0,0);
    Rect            ScrollViewport  = new Rect(0,0,0,0);
    Vector2         ScrollPosition  = Vector2.zero;
}
