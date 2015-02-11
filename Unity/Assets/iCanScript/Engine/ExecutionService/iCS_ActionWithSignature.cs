using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Subspace;

public abstract class iCS_ActionWithSignature : SSAction, iCS_ISignature {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    protected iCS_SignatureDataSource mySignature = null;
    
    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public object InInstance {
        get { return mySignature.InInstance; }
        set { mySignature.InInstance= value; }
    }
    public object OutInstance {
        get { return mySignature.OutInstance; }
    }
    public bool Trigger {
        get { return mySignature.Trigger; }
        set { mySignature.Trigger= value; }
    }
    public object GetValue(int idx) {
        return mySignature.GetValue(idx);
    }
    public void SetValue(int idx, object value) {
        mySignature.SetValue(idx, value);
    }
    public object ReturnValue {
        get { return mySignature.ReturnValue; }
        set { mySignature.ReturnValue= value; }
    }
    public void SetConnection(int idx, iCS_Connection connection) {
        mySignature.SetConnection(idx, connection);
    }
    public object this[int idx] {
        get { return mySignature[idx]; }
        set { mySignature[idx]= value; }
    }
    public object[] Parameters {
        get { return mySignature.Parameters; }
    }
    public iCS_Connection[] ParameterConnections {
        get { return mySignature.ParameterConnections; }
    }
    public int ParametersStart  { get { return mySignature.ParametersStart; }}
    public int ParametersEnd    { get { return mySignature.ParametersEnd; }}
    public bool IsParameterReady(int idx, int runId) {
        return mySignature.IsParameterReady(idx, runId);
    }
    public void UpdateParameter(int idx) {
        mySignature.UpdateParameter(idx);
    }
    public bool IsThisReady(int runId) {
        return mySignature.IsThisReady(runId);
    }
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_ActionWithSignature(iCS_VisualScriptImp visualScript, int priority, int nbOfParameters, int nbOfEnables)
    : base(visualScript, priority) {
        mySignature= new iCS_SignatureDataSource(nbOfParameters, nbOfEnables, this);
    }
    
    // ======================================================================
    // Implement ISignature delegate.
    // ----------------------------------------------------------------------
    public iCS_SignatureDataSource GetSignatureDataSource() { return mySignature; }
    public SSAction GetAction() { return this; }
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override void Execute(int runId) {
        // Don't execute if action disabled.
        if(!IsActive) {
            return;
        }
//        if(VisualScript.IsTraceEnabled) {
//            Debug.Log("Executing=> "+FullName+" ("+runId+")");            
//        }        

        // Clear the output trigger flag.
        mySignature.Trigger= false;
        // Wait until the enables can be resolved.
        bool isEnabled;
        if(!mySignature.GetIsEnabledIfReady(runId, out isEnabled)) {
			IsStalled= true;
//            if(VisualScript.IsTraceEnabled) {
//                Debug.Log("Executing=> "+FullName+" is waiting on the enables");
//            }
			return;
		}
		// Skip execution if this action is disabled.
        if(isEnabled == false) {
            MarkAsCurrent(runId);
            if(VisualScript.IsTraceEnabled) {
                Debug.Log("Executing=> "+FullName+" is disabled"+" ("+runId+")");
            }
            return;
        }
        // Invoke derived class to execute normally.
        IsStalled= true;
        DoExecute(runId);
        if(VisualScript.IsTraceEnabled) {    
            if(DidExecute(runId)) {
                Debug.Log("Executing=> "+FullName+" was executed sucessfully"+" ("+runId+")");
            }
//            else if(IsCurrent(runId)){
//                Debug.Log("Executing=> "+FullName+" is Current");
//            }
//            else {
//                Debug.Log("Executing=> "+FullName+" is waiting for an input");
//            }
        }
    }
    // ----------------------------------------------------------------------
    public override iCS_Connection GetStalledProducerPort(int runId) {
        if(IsCurrent(runId)) {
            return null;
        }
        return mySignature.GetStalledProducerPort(runId);
    }
    // ----------------------------------------------------------------------
    public override void ForceExecute(int runId) {
//#if UNITY_EDITOR
        if(VisualScript.IsTraceEnabled) {
            var stalledPort= GetStalledProducerPort(runId);
            var stalledPortName= stalledPort == null ? "" : stalledPort.PortFullName;
            if(stalledPort != null) {
                var stalledNode= stalledPort.Action;
                Debug.LogWarning("Force execute=> "+FullName+" STALLED PORT=> "+stalledPortName+" STALLED PORT NODE STATE=> "+stalledNode.IsCurrent(runId));            
                var stalledNodeParentId= stalledNode.ParentId;
                if(stalledNodeParentId > 1) {
                    var stalledNodeParent= VisualScript.RuntimeNodes[stalledNodeParentId] as iCS_ActionWithSignature;
                    if(stalledNodeParent != null) {
                        Debug.LogWarning("STALLED PORT NODE PARENT ENABLE=> "+stalledNodeParent.FullName+"("+stalledNodeParent.mySignature.GetIsEnabled()+")");
                    }
                }
            }
            Debug.LogWarning("Force Execute=> "+FullName);
        }
//#endif
        // Force verify enables.
        if(mySignature.GetIsEnabled() == false) {
            MarkAsCurrent(runId);
            return;
        }
        // Invoke derived class to force execute.
        IsStalled= true;
        DoForceExecute(runId);
    }
    // ----------------------------------------------------------------------
    // Override the execute marker to set the output trigger.
    public new void MarkAsExecuted(int runId) {
        mySignature.Trigger= true;
        base.MarkAsExecuted(runId);
    }
    // =========================================================================
    // Functions to override to provide specific behaviours.
    // ----------------------------------------------------------------------
    protected abstract void DoExecute(int runId);
    protected abstract void DoForceExecute(int runId);
}
