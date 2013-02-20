//#define NEW_ANIM
using UnityEngine;
using System;
using System.Collections;
using P=Prelude;

public partial class iCS_EditorObject {
    // ======================================================================
	// Fields
    // ----------------------------------------------------------------------
	private P.Animate<Vector2> AnimatedPosition= new P.Animate<Vector2>();
	private P.Animate<Vector2> AnimatedSize    = new P.Animate<Vector2>();

    // ======================================================================
    // Queries
    // ----------------------------------------------------------------------
    // Returns true if the display size is currently being animated.
    public bool IsDisplaySizeAnimated {
        get {
            return  AnimatedSize.IsActive;
        }
    }
    // ----------------------------------------------------------------------
    // Returns true if the display position is currently being animated.
    public bool IsDisplayPositionAnimated {
        get {
            return  AnimatedPosition.IsActive;            
        }
    }
    // ----------------------------------------------------------------------
    // Returns true if the display size or position are being animated.
    public bool IsAnimated {
        get {
            if(IsDisplaySizeAnimated) return true;
            return IsDisplayPositionAnimated;
        }
    }
    // ----------------------------------------------------------------------
	void SetStartValueForDisplayRectAnimation() {
		SetStartValueForDisplaySizeAnimation();
		SetStartValueForDisplayPositionAnimation();
	}
    // ----------------------------------------------------------------------
	void SetStartValueForDisplaySizeAnimation() {
		if(!IsDisplaySizeAnimated) {
			AnimatedSize.Reset(LayoutSize);
		}
	}
    // ----------------------------------------------------------------------
	void SetStartValueForDisplayPositionAnimation() {
		if(!IsDisplayPositionAnimated) {
			AnimatedPosition.Reset(GlobalLayoutPosition);
		}
	}
    // ----------------------------------------------------------------------
	void SetStartValueForDisplayRectAnimation(Rect r) {
		SetStartValueForDisplaySizeAnimation(new Vector2(r.width, r.height));
		SetStartValueForDisplayPositionAnimation(Math3D.Middle(r));
	}
    // ----------------------------------------------------------------------
	void SetStartValueForDisplaySizeAnimation(Vector2 startSize) {
		if(!IsDisplaySizeAnimated) {
			AnimatedSize.Reset(startSize);
		}
	}
    // ----------------------------------------------------------------------
	void SetStartValueForDisplayPositionAnimation(Vector2 startPos) {
		if(!IsDisplayPositionAnimated) {
			AnimatedPosition.Reset(startPos);
		}
	}
    // ----------------------------------------------------------------------
    public static float AnimationTimeFromPosition(Vector2 p1, Vector2 p2) {
        var distance= Vector2.Distance(p1,p2);
	    return AnimationTimeFromDistance(distance);
    }
    // ----------------------------------------------------------------------
    public static float AnimationTimeFromSize(Vector2 s1, Vector2 s2) {
        var distance= Vector2.Distance(s1,s2);
	    return AnimationTimeFromDistance(distance);
    }
    // ----------------------------------------------------------------------
    public static float AnimationTimeFromRect(Rect r1, Rect r2) {
        var distance= Vector2.Distance(new Vector2(r1.x,r1.y), new Vector2(r2.x,r2.y));
        var t= Vector2.Distance(new Vector2(r1.xMax,r1.y), new Vector2(r2.xMax,r2.y));
        if(t > distance) distance= t;
        t= Vector2.Distance(new Vector2(r1.xMax,r1.yMax), new Vector2(r2.xMax,r2.yMax));
        if(t > distance) distance= t;
        t= Vector2.Distance(new Vector2(r1.x,r1.yMax), new Vector2(r2.x,r2.yMax));
        if(t > distance) distance= t;        
	    return AnimationTimeFromDistance(distance);
    }
    // ----------------------------------------------------------------------
    public static float AnimationTimeFromDistance(float distance) {
	    return distance/iCS_PreferencesEditor.AnimationPixelsPerSecond;
    }
    // ----------------------------------------------------------------------
	public static P.TimeRatio BuildTimeRatioFromRect(Rect r1, Rect r2) {
	    float time= AnimationTimeFromRect(r1, r2);
		var timeRatio= new P.TimeRatio();
        timeRatio.Start(time);
		return timeRatio;
	}
    // ----------------------------------------------------------------------
	public static P.TimeRatio BuildTimeRatioFromPosition(Vector2 p1, Vector2 p2) {
	    float time= AnimationTimeFromPosition(p1, p2);
		var timeRatio= new P.TimeRatio();
        timeRatio.Start(time);
		return timeRatio;
	}
    // ----------------------------------------------------------------------
	public static P.TimeRatio BuildTimeRatioFromSize(Vector2 s1, Vector2 s2) {
	    float time= AnimationTimeFromSize(s1, s2);
		var timeRatio= new P.TimeRatio();
        timeRatio.Start(time);
		return timeRatio;
	}
    // ----------------------------------------------------------------------
	public static P.TimeRatio BuildTimeRatioFromDistance(float distance) {
	    float time= AnimationTimeFromDistance(distance);
		var timeRatio= new P.TimeRatio();
        timeRatio.Start(time);
		return timeRatio;
	}
    // ----------------------------------------------------------------------
	void StartDisplayRectAnimation() {
        var startRect= new Rect(AnimatedPosition.CurrentValue.x,
                                AnimatedPosition.CurrentValue.y,
                                AnimatedSize.CurrentValue.x,
                                AnimatedSize.CurrentValue.y);
		var timeRatio= BuildTimeRatioFromRect(startRect, GlobalLayoutRect);
		StartDisplayRectAnimation(timeRatio);
	}
    // ----------------------------------------------------------------------
	void StartDisplaySizeAnimation() {
        var startSize= AnimatedSize.CurrentValue;
		var timeRatio= BuildTimeRatioFromSize(startSize, LayoutSize); 
		StartDisplaySizeAnimation(timeRatio);
	}
    // ----------------------------------------------------------------------
	void StartDisplayPositionAnimation() {
        var startPos= AnimatedPosition.CurrentValue;
		var timeRatio= BuildTimeRatioFromPosition(startPos, GlobalLayoutPosition);
		StartDisplayPositionAnimation(timeRatio);
	}
    // ----------------------------------------------------------------------
	void StartDisplayRectAnimation(P.TimeRatio timeRatio) {
		StartDisplaySizeAnimation(timeRatio);
		StartDisplayPositionAnimation(timeRatio);
	}
    // ----------------------------------------------------------------------
	void StartDisplaySizeAnimation(P.TimeRatio timeRatio) {
		AnimatedSize.Start(AnimatedSize.CurrentValue,
							        LayoutSize,
							        timeRatio,
                                    (start,end,ratio)=>Math3D.Lerp(start,end,ratio));
	}
    // ----------------------------------------------------------------------
	void StartDisplayPositionAnimation(P.TimeRatio timeRatio) {
		AnimatedPosition.Start(AnimatedPosition.CurrentValue,
										GlobalLayoutPosition,
										timeRatio,
		                                (start,end,ratio)=>Math3D.Lerp(start,end,ratio));
	}
    // ----------------------------------------------------------------------
	public void UpdateAnimation() {
#if NEW_ANIM
        if(AnimatedSize.IsActive) {
        	if(AnimatedSize.IsElapsed) {
        		AnimatedSize.Reset();
        	} else {
        		AnimatedSize.Update();
        	}
        	LayoutSize= AnimatedSize.CurrentValue;
        }
        if(AnimatedPosition.IsActive) {
        	if(AnimatedPosition.IsElapsed) {
        		AnimatedPosition.Reset();
        	} else {
        		AnimatedPosition.Update();
        	}
        	GlobalLayoutPosition= AnimatedPosition.CurrentValue;
        }
#else
		if(AnimatedSize.IsActive) {
			if(AnimatedSize.IsElapsed) {
				AnimatedSize.Reset(LayoutSize);
			} else {
				AnimatedSize.Update();
			}
		}
		if(AnimatedPosition.IsActive) {
			if(AnimatedPosition.IsElapsed) {
				AnimatedPosition.Reset(GlobalLayoutPosition);
			} else {
				AnimatedPosition.Update();
			}
		}
#endif
	}

    // ----------------------------------------------------------------------
    public float DisplayAlpha {
        get {
            if(IsPort)      return ParentNode.DisplayAlpha;
            if(!IsAnimated) {
                myInvisibleBeforeAnimation= false;
                return 1f;
            }
            if(!IsVisibleInLayout) {
                return 1f-AnimatedPosition.Ratio;
            }
            if(myInvisibleBeforeAnimation) {
                return AnimatedPosition.Ratio;
            }
            return 1f;
        }
    }
    
}
