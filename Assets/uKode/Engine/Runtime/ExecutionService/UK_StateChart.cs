using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class UK_StateChart : UK_Action {
    // ======================================================================
    // Internal types
    // ----------------------------------------------------------------------
    enum ExecutionState { VerifyingTransition, RunningEntry, RunningExit, RunningUpdate };
    
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    UK_State        myEntryState      = null;
    List<UK_State>  myActiveStack     = new List<UK_State>();
    List<UK_State>  myChildren        = new List<UK_State>();
    int             myQueueIdx        = 0;
    UK_State        myNextState       = null;
    UK_State        myTransitionParent= null;
    int             myEntrySize       = -1;
    ExecutionState  myExecutionState  = ExecutionState.VerifyingTransition;
    
    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public UK_State EntryState {
        get { return myEntryState; }
        set { myEntryState= value; }
    }
    public UK_State ActiveState {
        get {
            int end= myActiveStack.Count;
            return end == 0 ? null : myActiveStack[end-1];
        }
    }
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public UK_StateChart(string name) : base(name) {
    }

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    public override void Execute(int frameId) {
        // Make certain that at least one active state exists.
        if(myActiveStack.Count == 0 && myEntryState != null) MoveToState(myEntryState, frameId);
        // Process any active transition.
        if(myExecutionState == ExecutionState.VerifyingTransition) {
            ExecuteTransitions(frameId);            
        }
        // Execute state exit functions.
        if(myExecutionState == ExecutionState.RunningExit) {
            ExecuteExits(frameId);
        }
        // Execute state update functions.
        if(myExecutionState == ExecutionState.RunningEntry) {
            ExecuteEntries(frameId);
        }
        // Execute state update functions.
        if(myExecutionState == ExecutionState.RunningUpdate) {
            ExecuteUpdates(frameId);
        }
    }
    public override void ForceExecute(int frameId) {
        Execute(frameId);
    }
    
    // ----------------------------------------------------------------------
    void ExecuteTransitions(int frameId) {
        // Determine if a transition exists for one of the active states.
        UK_State state= null;
        int end= myActiveStack.Count;
        while(myQueueIdx < end) {
            state= myActiveStack[myQueueIdx];
            UK_VerifyTransitions transitions= state.Transitions;
            transitions.Execute(frameId);
            if(!transitions.IsCurrent(frameId)) {
                if(!transitions.IsStalled) {
                    IsStalled= false;
                }
                return;
            }
            IsStalled= false;
            UK_Transition firedTransition= transitions.TriggeredTransition;
            if(firedTransition != null && state != ActiveState) {
                MoveToState(state, frameId);
                return;
            }
            ++myQueueIdx;
        }
        IsStalled= false;
        myQueueIdx= 0;
        myExecutionState= ExecutionState.RunningUpdate;
    }
    // ----------------------------------------------------------------------
    void ExecuteUpdates(int frameId) {
        bool stalled= true;
        while(myQueueIdx < myActiveStack.Count) {
            UK_State state= myActiveStack[myQueueIdx];
            UK_Action action= state.OnUpdateAction;
            action.Execute(frameId);            
            if(!action.IsCurrent(frameId)) {
                // Verify if the child is a staled dispatcher.
                if(!action.IsStalled) {
                    stalled= false;
                }                    
                IsStalled= stalled;
                return;
            }
            stalled= false;
            ++myQueueIdx;
        }
        // Reset iterators for next frame.
        myQueueIdx= 0;
        MarkAsCurrent(frameId);
    }
    // ----------------------------------------------------------------------
    void ExecuteExits(int frameId) {
        for(; myQueueIdx >= 0; --myQueueIdx) {
            UK_State state= myActiveStack[myQueueIdx];
            if(state == myTransitionParent) break;
            state.OnExit(frameId);
        }
        // Update active stack state.
        int stackSize= myActiveStack.Count;
        int stableSize= myQueueIdx+1;
        int newSize= stableSize+myEntrySize;
        if(newSize < stackSize) myActiveStack.RemoveRange(newSize, stackSize-newSize);
        if(newSize > myActiveStack.Capacity) myActiveStack.Capacity= newSize;
        while(myActiveStack.Count < newSize) myActiveStack.Add(null);
        UK_State toAdd= myNextState;
        for(int offset= myEntrySize-1; offset >= 0; --offset) {
            myActiveStack[stableSize+offset]= toAdd;
            toAdd= toAdd.ParentState;            
        }
        // Prepare to execute entry
        myExecutionState= ExecutionState.RunningEntry;
        myQueueIdx= stableSize;
    }
    // ----------------------------------------------------------------------
    void ExecuteEntries(int frameId) {
        // Execute entry functions.
        int newSize= myActiveStack.Count;
        for(; myQueueIdx < newSize; ++myQueueIdx) {
            myActiveStack[myQueueIdx].OnEntry(frameId);
        }
        // Prepare to execute update functions
        myExecutionState= ExecutionState.RunningUpdate;
        myQueueIdx= 0;        
    }
    // ----------------------------------------------------------------------
    void MoveToState(UK_State newState, int frameId) {
        myNextState= newState;
        int stackSize= myActiveStack.Count;
        // Determine transition parent node
        myTransitionParent= null;
        UK_State toTest= newState;
        myEntrySize= -1;
        int idx;
        do {
            ++myEntrySize;
            for(idx= stackSize-1; idx >= 0; --idx) {
                if(myActiveStack[idx] == toTest) {
                    myTransitionParent= toTest;
                    break;
                }
            }
            toTest= toTest.ParentState;
            if(toTest == null) {
                ++myEntrySize;
                break;
            }
        } while(myTransitionParent == null);
        // Prepare to execute exit functions.
        myExecutionState= ExecutionState.RunningExit;
        myQueueIdx= stackSize-1;
    }
    
    // ======================================================================
    // Child Management
    // ----------------------------------------------------------------------
    public void AddChild(UK_Object _object) {
        UK_State state= _object as UK_State;
        if(state != null) {
            if(myChildren.Count == 0) myEntryState= state;
            myChildren.Add(state);
        }
    }
    public void RemoveChild(UK_Object _object) {
        UK_State state= _object as UK_State;
        if(state != null) {
            if(state == myEntryState) myEntryState= null;
            myChildren.Remove(state);
        }
    }
}
