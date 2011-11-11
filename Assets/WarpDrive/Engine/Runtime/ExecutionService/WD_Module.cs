using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WD_Module : WD_Action {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    List<WD_Action> myExecuteQueue= new List<WD_Action>();
    int             myQueueIdx= 0;
    int             myNbOfTries= 0;
    object[]        myInParams      = new object[0];
    object[]        myOutParams     = new object[0];
    WD_Connection[] myInConnections = new WD_Connection[0];
    WD_Connection[] myOutConnections= new WD_Connection[0];
    
    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public object this[int idx] {
        get {
            int inLen= myInParams.Length;
            if(idx < inLen) return myInParams[idx];
            idx-= inLen;
            if(idx < myOutParams.Length) return myOutParams[idx];
            Debug.LogError("Invalid parameter index given");
            return null;
        }
        set {
            int inLen= myInParams.Length;
            if(idx < inLen) { myInParams[idx]= value; return; }
            idx-= inLen;
            if(idx < myOutParams.Length) { myOutParams[idx]= value; return; }
            Debug.LogError("Invalid parameter index given");            
        }
    }
        
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public WD_Module(string name) : base(name) {}
    public void SetConnections(object[] inParams, WD_Connection[] inConnections, object[] outParams, WD_Connection[] outConnections) {
        myInParams      = inParams;
        myInConnections = inConnections;
        myOutParams     = outParams;
        myOutConnections= outConnections;
    }
    
    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override void Execute(int frameId) {
        // Verify that we are ready to run.
        foreach(var c in myInConnections) {
            if(c.IsConnected && !c.IsReady(frameId)) return;
        }
        // Fetch all the inputs.
        for(int i= 0; i < myInConnections.Length; ++i) {
            if(myInConnections[i].IsConnected) {
                myInParams[i]= myInConnections[i].Value;
            }
        }
        // Attempt to execute child functions.
        int maxTries= myExecuteQueue.Count; maxTries= 1+(maxTries*maxTries+maxTries)/2;
        for(; myQueueIdx < myExecuteQueue.Count && myNbOfTries < maxTries; ++myNbOfTries) {
            WD_Action action= myExecuteQueue[myQueueIdx];
            action.Execute(frameId);            
            if(!action.IsCurrent(frameId)) {
                // The function is not ready to execute so lets delay the execution.
                myExecuteQueue.RemoveAt(myQueueIdx);
                myExecuteQueue.Add(action);
                return;
            }
            ++myQueueIdx;
        }
        // Verify that the graph is not looping.
        if(myNbOfTries >= maxTries) {
            Debug.LogError("Execution of graph is looping!!! "+myExecuteQueue[myQueueIdx].Name+":"+myExecuteQueue[myQueueIdx].GetType().Name+" is included in the loop. Please break the cycle and retry.");
        }
        // Update all outputs.
        for(int i= 0; i < myOutConnections.Length; ++i) {
            if(myOutConnections[i].IsConnected) {
                myOutParams[i]= myOutConnections[i].Value;
            }
        }        
        // Reset iterators for next frame.
        myQueueIdx= 0;
        myNbOfTries= 0;
        MarkAsCurrent(frameId);
    }

    // ----------------------------------------------------------------------
    // Returns true if the object is an executable that we support.
    bool IsExecutable(object _object) {
        return _object is WD_Action;
    }
    

    // ======================================================================
    // Connector Management
    // ----------------------------------------------------------------------
    public void AddChild(object obj) {
        if(IsExecutable(obj)) {
            myExecuteQueue.Add(obj as WD_Action);
        }
    }
    public void RemoveChild(object obj) {
        if(IsExecutable(obj)) {
            myExecuteQueue.Remove(obj as WD_Action);
        }
    }
}
