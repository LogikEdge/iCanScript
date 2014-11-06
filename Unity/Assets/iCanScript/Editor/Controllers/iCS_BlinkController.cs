﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using P=Prelude;
using TS=iCS_TimerService;

public static class iCS_BlinkController {
    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    static iCS_BlinkController()    {
        myAnimationTimer.Schedule();
    }
    public static void Start()      {}
    public static void Shutdown() {
        myAnimationTimer.Stop();
    }

    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    static TS.TimedAction   myAnimationTimer  = TS.CreateTimedAction(0.05f, DoAnimation, /*isLooping=*/true);
    static P.Animate<float> mySlowBlink   = new P.Animate<float>();
    static P.Animate<float> myNormalBlink = new P.Animate<float>();
    static P.Animate<float> myFastBlink   = new P.Animate<float>();
    

    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    public static float SlowBlinkRatio   { get { return mySlowBlink.CurrentValue; }}
    public static float NormalBlinkRatio { get { return myNormalBlink.CurrentValue; }}
    public static float FastBlinkRatio   { get { return myFastBlink.CurrentValue; }}
    public static Color SlowBlinkColor   { get { return new Color(1f,1f,1f,SlowBlinkRatio); }}
    public static Color NormalBlinkColor { get { return new Color(1f,1f,1f,NormalBlinkRatio); }}
    public static Color FastBlinkColor   { get { return new Color(1f,1f,1f,FastBlinkRatio); }}
    
    // ----------------------------------------------------------------------
    static void DoAnimation() {
		// -- Restart the alpha animation --
		if(mySlowBlink.IsElapsed) {
			mySlowBlink.Start(0f, 3f, 3f, (start,end,ratio)=> 0.67f*Mathf.Abs(1.5f-Math3D.Lerp(start,end,ratio)));
		}
		if(myNormalBlink.IsElapsed) {
			myNormalBlink.Start(0f, 2f, 2f, (start,end,ratio)=> Mathf.Abs(1f-Math3D.Lerp(start,end,ratio)));
		}
		if(myFastBlink.IsElapsed) {
			myFastBlink.Start(0f, 1.5f, 1.5f, (start,end,ratio)=> 1.33f*Mathf.Abs(0.75f-Math3D.Lerp(start,end,ratio)));
		}
		// Animate the error display alpha.
		mySlowBlink.Update();
		myNormalBlink.Update();
		myFastBlink.Update();
    }
}
