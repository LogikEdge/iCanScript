﻿using UnityEngine;
using System;
using System.Collections;
using Subspace;

public class iCS_DynamicUserFunctionCall : SSActionWithSignature {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    protected SSActionWithSignature myUserAction= null;
              bool                  isActionOwner= false;

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_DynamicUserFunctionCall(string name, SSObject parent, int priority,
                                       int nbOfParameters, int nbOfEnables)
    : base(name, parent, priority, nbOfParameters, nbOfEnables) {
        Debug.Log("Creating a dynamic user function call");
    }

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoExecute() {
//#if UNITY_EDITOR
        try {
//#endif
            // Wait until this port is ready.
            if(IsThisReady(myContext.RunId)) {
                // Fetch the user action.
                var gameObject= This as GameObject;
                if(gameObject == null) {
                    Debug.LogWarning("iCanScript: Unable to find game object with variable: "+FullName);
                    MarkAsCurrent();
                }
                var vs= gameObject.GetComponent(typeof(iCS_VisualScriptImp)) as iCS_VisualScriptImp;
                if(vs == null) {
                    Debug.LogWarning("iCanScript: Unable to find visual script that contains variable: "+FullName+" in game object: "+gameObject.name);
                    MarkAsCurrent();
                }
                var variableObject= vs.GetPublicInterfaceFromName(Name);
                if(variableObject == null) {
                    Debug.LogWarning("iCanScript: Unable to find variable: "+FullName+" in visual script of game object: "+gameObject.name);
                    MarkAsCurrent();
                }
                var myUserAction= vs.RuntimeNodes[variableObject.InstanceId] as SSActionWithSignature;
    
                // Wait until all inputs are ready.
                var parameterLen= Parameters.Length;
                for(int i= 0; i < parameterLen; ++i) {
                    if(IsParameterReady(i, myContext.RunId) == false) {
                        return;
                    }
                }
                // Fetch all parameters.
                for(int i= 0; i < parameterLen; ++i) {
                    UpdateParameter(i);
                }
                // Copy input ports
                var parameters= Parameters;
                var userActionParameters= myUserAction.Parameters;
                for(int i= 0; i < parameterLen; ++i) {
                    userActionParameters[i]= parameters[i];
                }
                // Wait until user function becomes available
                if(!isActionOwner && myUserAction.IsActive == true) {
                    IsStalled= false;
                    return;
                }
                // Execute associated function.
                myUserAction.IsActive= true;
                if(!isActionOwner) {
                    isActionOwner= true;
                    myUserAction.Context.RunId= myUserAction.Context.RunId+1;
                }
                myUserAction.Evaluate();
                // Copy output ports
                for(int i= 0; i < parameterLen; ++i) {
    				UpdateParameter(i);
    			}
                // Reflection the action run status.
                IsStalled= myUserAction.IsStalled;
                if(myUserAction.DidExecute()) {
                    isActionOwner= false;
                    myUserAction.IsActive= false;
                    MarkAsExecuted();
                }
                else if(myUserAction.IsCurrent){
                    isActionOwner= false;
                    myUserAction.IsActive= false;
                    MarkAsCurrent();
                }            
            }
//#if UNITY_EDITOR
        }
        catch(Exception e) {
            Debug.LogWarning("iCanScript: Exception throw in  "+FullName+" => "+e.Message);
            string thisName= (This == null ? "null" : This.ToString());
            string parametersAsStr= "";
            int nbOfParams= Parameters.Length;
            if(nbOfParams != 0) {
                for(int i= 0; i < nbOfParams; ++i) {
                    parametersAsStr+= Parameters[i].ToString();
                    if(i != nbOfParams-1) {
                        parametersAsStr+=", ";
                    }
                }
            }
            Debug.LogWarning("iCanScript: while invoking => "+thisName+"."+Name+"("+parametersAsStr+")");
            if(isActionOwner) {
                isActionOwner= false;
                myUserAction.IsActive= false;                
            }
            MarkAsCurrent();
        }
//#endif
    }

    // ----------------------------------------------------------------------
    protected override void DoForceExecute() {
//#if UNITY_EDITOR
        try {
//#endif
            // Fetch the user action.
            var gameObject= This as GameObject;
            if(gameObject == null) {
                Debug.LogWarning("iCanScript: Unable to find game object with variable: "+FullName);
                MarkAsCurrent();
            }
            var vs= gameObject.GetComponent(typeof(iCS_VisualScriptImp)) as iCS_VisualScriptImp;
            if(vs == null) {
                Debug.LogWarning("iCanScript: Unable to find visual script that contains variable: "+FullName+" in game object: "+gameObject.name);
                MarkAsCurrent();
            }
            var variableObject= vs.GetPublicInterfaceFromName(Name);
            if(variableObject == null) {
                Debug.LogWarning("iCanScript: Unable to find variable: "+FullName+" in visual script of game object: "+gameObject.name);
                MarkAsCurrent();
            }
            var myUserAction= vs.RuntimeNodes[variableObject.InstanceId] as SSActionWithSignature;

            // Fetch all parameters.
            var parameterLen= Parameters.Length;
            for(int i= 0; i < parameterLen; ++i) {
                UpdateParameter(i);
            }
            // Copy input ports
            var parameters= Parameters;
            var userActionParameters= myUserAction.Parameters;
            for(int i= 0; i < parameterLen; ++i) {
                userActionParameters[i]= parameters[i];
            }
            // Wait unitil user function becomes available
            if(!isActionOwner && myUserAction.IsActive == true) {
                IsStalled= false;
                return;
            }
            // Execute associated function.
            myUserAction.IsActive= true;
            if(!isActionOwner) {
                isActionOwner= true;
                myUserAction.Context.RunId= myUserAction.Context.RunId+1;
            }
            myUserAction.Evaluate();
            // Copy output ports
            for(int i= 0; i < parameterLen; ++i) {
				UpdateParameter(i);
			}
            // Reflection the action run status.
            IsStalled= myUserAction.IsStalled;
            if(myUserAction.DidExecute()) {
                isActionOwner= false;
                myUserAction.IsActive= false;
                MarkAsExecuted();
            }
            else if(myUserAction.IsCurrent){
                isActionOwner= false;
                myUserAction.IsActive= false;
                MarkAsCurrent();
            }            
//#if UNITY_EDITOR
        }
        catch(Exception e) {
            Debug.LogWarning("iCanScript: Exception throw in  "+FullName+" => "+e.Message);
            string thisName= (This == null ? "null" : This.ToString());
            string parametersAsStr= "";
            int nbOfParams= Parameters.Length;
            if(nbOfParams != 0) {
                for(int i= 0; i < nbOfParams; ++i) {
                    parametersAsStr+= Parameters[i].ToString();
                    if(i != nbOfParams-1) {
                        parametersAsStr+=", ";
                    }
                }
            }
            Debug.LogWarning("iCanScript: while invoking => "+thisName+"."+Name+"("+parametersAsStr+")");
            if(isActionOwner) {
                isActionOwner= false;
                myUserAction.IsActive= false;                
            }
            MarkAsCurrent();
        }
//#endif
    }
}
