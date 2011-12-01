using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class UK_DispatcherBase : UK_Action, UK_IDispatcher {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    protected List<UK_Action> myExecuteQueue= new List<UK_Action>();
    protected int             myQueueIdx = 0;
    protected bool            myIsStalled= false;
    
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public bool IsStalled { get { return myIsStalled; }}
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public UK_DispatcherBase(string name) : base(name) {}

    // ======================================================================
    // IDispatcher implementation
    // ----------------------------------------------------------------------
    public UK_DispatcherBase GetDispatcher()    { return this; }
    
    // ======================================================================
    // Queue Management
    // ----------------------------------------------------------------------
    public void AddChild(UK_Action action) {
        myExecuteQueue.Add(action);
    }
    public void RemoveChild(UK_Action action) {
        myExecuteQueue.Remove(action);
    }
}
