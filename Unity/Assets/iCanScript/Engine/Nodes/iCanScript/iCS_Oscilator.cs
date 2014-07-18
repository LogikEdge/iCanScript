﻿using UnityEngine;
using System.Collections;

[iCS_Class(Company="iCanScript", Icon= "iCS_ClockIcon.psd")]
public class iCS_Oscillator {
    bool    myActiveState= false;
    float   myTrueElapseTime;
    float   myFalseElapseTime;
        
    [iCS_Function]
    public iCS_Oscillator() {}
    
    [iCS_Function(Return="out")]
    public bool Oscillate(float trueTime, float falseTime, bool active, out bool invOut) {
        if(active == false) {
            myActiveState= false;
            invOut= true;
            return false;
        }
        var time= Time.time;
        if(myActiveState == false) {
            myActiveState= true;
            myTrueElapseTime= time+trueTime;
            myFalseElapseTime= myTrueElapseTime+falseTime;
            invOut= false;
            return true;
        }
        if(myTrueElapseTime < myFalseElapseTime) {
            if(time < myTrueElapseTime) {
                invOut= false;
                return true;
            }
            myTrueElapseTime= myFalseElapseTime+trueTime;
            invOut= true;
            return false;
        }
        if(time < myFalseElapseTime) {
            invOut= true;
            return false;
        }
        myFalseElapseTime= myTrueElapseTime+falseTime;
        invOut= false;
        return true;
    }
}
