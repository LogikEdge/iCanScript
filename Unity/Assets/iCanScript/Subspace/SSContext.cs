///////////////////////////////////////////////////////////
//  SSContext.cs
//  Implementation of the Class SSContext
//  Generated by Enterprise Architect
//  Created on:      12-Feb-2015 11:01:08 AM
//  Original author: Reinual
///////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;


namespace Subspace {

    public class SSContext {
        // ======================================================================
        // Fields
        // ----------------------------------------------------------------------
        int                         myRunId          = 0;
        Object                      myUserData	     = null;
    	bool                        myIsTraceEnabled = false;
        Action<string, SSObject>    myErrorDelegate  = null;
        Action<string, SSObject>    myWarningDelegate= null;

        // ======================================================================
        // Properties
        // ----------------------------------------------------------------------
        public int RunId {
            get { return myRunId; }
            set { myRunId= value;}
        }
        public Object UserData {
            get { return myUserData; }
            set { myUserData= value; }
        }
        public bool IsTraceEnabled {
            get { return myIsTraceEnabled; }
            set { myIsTraceEnabled= value; }
        }
        public Action<string, SSObject> ErrorDelegate {
            get { return myErrorDelegate; }
            set { myErrorDelegate= value; }
        }
        public Action<string, SSObject> WarningDelegate {
            get { return myWarningDelegate; }
            set { myWarningDelegate= value; }
        }
        
        // ======================================================================
        // Constructor/Destructor
        // ----------------------------------------------------------------------
        public SSContext(Object userData= null) {
            myUserData= userData;
        }
        public SSContext Clone() {
            var clone= new SSContext();
            clone.CopyFrom(this);
            return clone;
        }
        public void CopyFrom(SSContext theOther) {
            myRunId          = theOther.myRunId;
            myUserData	     = theOther.myUserData;
        	myIsTraceEnabled = theOther.myIsTraceEnabled;
            myErrorDelegate  = theOther.myErrorDelegate;
            myWarningDelegate= theOther.myWarningDelegate;
        }
        
        // ======================================================================
        // Error/Warning Report
        // ----------------------------------------------------------------------
        public void ReportError(string msg, SSObject obj) {
            if(myErrorDelegate != null) {
                myErrorDelegate(msg, obj);
            }
        }
        public void ReportWarning(string msg, SSObject obj) {
            if(myWarningDelegate != null) {
                myWarningDelegate(msg, obj);
            }
        }
        
    }//end SSContext    

}
