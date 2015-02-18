using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Subspace;

public sealed class iCS_StateChart : SSNodeAction {
    // ======================================================================
    // Internal types
    // ----------------------------------------------------------------------
    enum ExecutionState { VerifyingTransition, RunningEntry, RunningExit, RunningUpdate };
    
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    iCS_State        		myEntryState      = null;
    List<iCS_State>  		myActiveStack     = new List<iCS_State>();
    List<iCS_State>  		myChildren        = new List<iCS_State>();
	iCS_ParallelDispatcher	myDispatcher      = null;
    int              		myQueueIdx        = 0;
    iCS_Transition   		myFiredTransition = null;
    iCS_State        		myNextState       = null;
    iCS_State        		myTransitionParent= null;
    int              		myEntrySize       = -1;
    ExecutionState   		myExecutionState  = ExecutionState.VerifyingTransition;
    
    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public iCS_State EntryState {
        get { return myEntryState; }
        set { myEntryState= value; }
    }
    public iCS_State ActiveState {
        get {
            int end= myActiveStack.Count;
            return end == 0 ? null : myActiveStack[end-1];
        }
    }
    public bool IsActiveState(iCS_State state) {
        foreach(var s in myActiveStack) {
            if(s == state) return true;
        }
        return false;
    }
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_StateChart(string name, SSObject parent, int priority, int nbOfParams, int nbOfEnables)
    : base(name, parent, priority, nbOfParams, nbOfEnables) {
    	myDispatcher= new iCS_ParallelDispatcher(name, this, priority, 0, 0);
    }

    // ======================================================================
    // Execution
    // ----------------------------------------------------------------------
    protected override void DoEvaluate() {
        // Make certain that at least one active state exists.
        if(myActiveStack.Count == 0 && myEntryState != null) {
            var entryState= myEntryState;
            while(entryState.EntryState != null) entryState= entryState.EntryState;
            MoveToState(entryState);
        }
        // Process any active transition.
        if(myExecutionState == ExecutionState.VerifyingTransition) {
            ExecuteVerifyTransitions();            
        }
        // Execute state exit functions.
        if(myExecutionState == ExecutionState.RunningExit) {
            ExecuteExits();
        }
        // Execute state entry functions.
        if(myExecutionState == ExecutionState.RunningEntry) {
            ExecuteEntries();
        }
        // Execute state update functions.
        if(myExecutionState == ExecutionState.RunningUpdate) {
            ExecuteUpdates();
        }
		// Attempt to execute all other functions (packge like)
		if(!myDispatcher.IsEvaluated) {
			myDispatcher.Evaluate();			
		}
    }
    // ----------------------------------------------------------------------
    // TODO: GetStalledProducerPort
    public override SSPullBinding GetStalledProducerPort() {
        // Process any active transition.
        if(myExecutionState == ExecutionState.VerifyingTransition) {
            var producerPort= GetStalledProducerPortInTransitions();            
            if(producerPort != null) {
                return producerPort;
            }
        }
        // Execute state exit functions.
        if(myExecutionState == ExecutionState.RunningExit) {
            var producerPort= GetStalledProducerPortOnExit();
            if(producerPort != null) {
                return producerPort;
            }
        }
        // Execute state entry functions.
        if(myExecutionState == ExecutionState.RunningEntry) {
            var producerPort= GetStalledProducerPortOnEntry();
            if(producerPort != null) {
                return producerPort;
            }
        }
        // Execute state update functions.
        if(myExecutionState == ExecutionState.RunningUpdate) {
            var producerPort= GetStalledProducerPortOnUpdate();
            if(producerPort != null) {
                return producerPort;
            }
        }
		// Execute all other functions (packge like)
		if(!myDispatcher.IsEvaluated) {
			var producerPort= myDispatcher.GetStalledProducerPort();			
            if(producerPort != null) {
                return producerPort;
            }
		}
        return null;
    }
    // ----------------------------------------------------------------------
    protected override void DoExecute() {
        // Process any active transition.
        if(myExecutionState == ExecutionState.VerifyingTransition) {
            ExecuteVerifyTransitions(/*forced=*/true);            
        }
        // Execute state exit functions.
        if(myExecutionState == ExecutionState.RunningExit) {
            ExecuteExits(/*forced=*/true);
        }
        // Execute state entry functions.
        if(myExecutionState == ExecutionState.RunningEntry) {
            ExecuteEntries(/*forced=*/true);
        }
        // Execute state update functions.
        if(myExecutionState == ExecutionState.RunningUpdate) {
            ExecuteUpdates(/*forced=*/true);
        }
		// Execute all other functions (packge like)
		if(!myDispatcher.IsEvaluated) {
			myDispatcher.Execute();			
		}
    }
    
    // ----------------------------------------------------------------------
	// The transition verification are ran unorderly according to their
	// readyiness.  The verification is completed once one transition
	// trigger has fired or all transition have been verified.
    void ExecuteVerifyTransitions(bool forced= false) {
		// Remove any pending triggers.
		myFiredTransition= null;
        // Determine if a transition exists for one of the active states.
        int end= myActiveStack.Count;
		for(int idx= myQueueIdx; idx < end; ++idx) {
            iCS_State state= myActiveStack[idx];
            iCS_VerifyTransitions transitions= state.Transitions;
			// Transition has already been tested.  Just move on to next one.
			if(transitions.IsEvaluated) {
				if(idx == myQueueIdx) {
					++myQueueIdx;
				}					
                IsStalled= false;
				continue;
			}
			// Verify transition.
			if(forced) {
	            transitions.Execute();					
			} else {
	            transitions.Evaluate();						
			}
            if(transitions.IsEvaluated) {
	            myFiredTransition= transitions.TriggeredTransition;
	            if(myFiredTransition != null && myFiredTransition.EndState != ActiveState) {
					IsStalled= false;
                    var newState= myFiredTransition.EndState;
                    while(newState.EntryState != null) newState= newState.EntryState;
	                MoveToState(newState);
	                return;
	            }
				if(idx == myQueueIdx) {
					++myQueueIdx;
				}
				IsStalled= false;
				continue;
			}
			IsStalled&= transitions.IsStalled;
		}
		// Not all transitions have ran.
		if(myQueueIdx < end) {
			// Update stalled indication.
			return;
		}
		// All transition have ran & none have triggered.
        IsStalled= false;
        myQueueIdx= 0;
        myExecutionState= ExecutionState.RunningUpdate;
    }
    // ----------------------------------------------------------------------
    SSPullBinding GetStalledProducerPortInTransitions() {
        int end= myActiveStack.Count;
		for(int idx= myQueueIdx; idx < end; ++idx) {
            iCS_State state= myActiveStack[idx];
            iCS_VerifyTransitions transitions= state.Transitions;
			// Transition has already been tested.  Just move on to next one.
			if(!transitions.IsEvaluated) {
                var producerPort= transitions.GetStalledProducerPort();
                if(producerPort != null) {
                    return producerPort;
                }
			}
		}
        return null;        
    }            
    // ----------------------------------------------------------------------
	// The state updates are ran unorderly according to their readyiness.
    void ExecuteUpdates(bool forced= false) {
		// Run the update of each active state.
        int end= myActiveStack.Count;
		for(int idx= myQueueIdx; idx < end; ++idx) {
            iCS_State state= myActiveStack[idx];
            SSAction action= state.OnUpdateAction;
			// Update is not needed or already ran.  Just move to the next state...
			if(action == null || action.IsEvaluated) {
				if(idx == myQueueIdx) {
					++myQueueIdx;
				}
                IsStalled= false;
				continue;
			}
			// Run the update action.
			if(forced) {
				action.Execute();
			} else {
                action.Evaluate();            						
			}
            if(action.IsEvaluated) {
				if(idx == myQueueIdx) {
					++myQueueIdx;
				}
				IsStalled= false;
			}
            else {
                IsStalled&= action.IsStalled;                
            }
		}
		// Not all updates have ran.
		if(myQueueIdx < end) {
			return;
		}
        // Reset iterators for next frame.
        IsStalled= false;
        myQueueIdx= 0;
        myExecutionState= ExecutionState.VerifyingTransition;
        myFiredTransition= null;            
        MarkAsExecuted();
    }
    // ----------------------------------------------------------------------
    SSPullBinding GetStalledProducerPortOnUpdate() {
        int end= myActiveStack.Count;
		for(int idx= myQueueIdx; idx < end; ++idx) {
            iCS_State state= myActiveStack[idx];
            SSAction action= state.OnUpdateAction;
			// Update is not needed or already ran.  Just move to the next state...
			if(action != null && !action.IsEvaluated) {
                var producerPort= action.GetStalledProducerPort();
                if(producerPort != null) {
                    return producerPort;
                }
			}
		}
        return null;        
    }            

    // ----------------------------------------------------------------------
	// The state exits are ran orderly from the inner state towards the
	// outter state.
    void ExecuteExits(bool forced= false) {
		// Run the OnExist functions until the common state of the transition.
		for(; myQueueIdx >= 0; --myQueueIdx) {
            iCS_State state= myActiveStack[myQueueIdx];
            if(state == myTransitionParent) break;
            SSAction action= state.OnExitAction;
			if(action != null && !action.IsEvaluated) {
				if(forced) {
	                action.Execute();            
				} else {
	                action.Evaluate();		
				}
                if(action.IsEvaluated) {
					IsStalled= false;
				} else {
                    IsStalled&= action.IsStalled;
					return;
				}
			}
		}
        // Update active stack state.
		IsStalled= false;
		UpdateActiveStack();
    }
    // ----------------------------------------------------------------------
    SSPullBinding GetStalledProducerPortOnExit() {
        int end= myActiveStack.Count;
		for(int idx= myQueueIdx; idx < end; ++idx) {
            iCS_State state= myActiveStack[idx];
            SSAction action= state.OnExitAction;
			// Update is not needed or already ran.  Just move to the next state...
			if(action != null && !action.IsEvaluated) {
                var producerPort= action.GetStalledProducerPort();
                if(producerPort != null) {
                    return producerPort;
                }
			}
		}
        return null;        
    }            
    // ----------------------------------------------------------------------
	// The state entries are ran orderly from the outter state towards the
	// inner state.
    void ExecuteEntries(bool forced= false) {
        int end= myActiveStack.Count;
		for(; myQueueIdx < end; ++myQueueIdx) {
            iCS_State state= myActiveStack[myQueueIdx];
            SSAction action= state.OnEntryAction;
			if(action != null && !action.IsEvaluated) {
				if(forced) {
	                action.Execute();            
				} else {
	                action.Evaluate();            						
				}
                if(action.IsEvaluated) {
					IsStalled= false;
				} else {
                    IsStalled&= action.IsStalled;
					return;					
				}
			}
		}
        // Prepare to execute update functions
		IsStalled= false;
        myExecutionState= ExecutionState.RunningUpdate;
        myQueueIdx= 0;        
    }
    // ----------------------------------------------------------------------
    SSPullBinding GetStalledProducerPortOnEntry() {
        int end= myActiveStack.Count;
		for(int idx= myQueueIdx; idx < end; ++idx) {
            iCS_State state= myActiveStack[idx];
            SSAction action= state.OnEntryAction;
			// Update is not needed or already ran.  Just move to the next state...
			if(action != null && !action.IsEvaluated) {
                var producerPort= action.GetStalledProducerPort();
                if(producerPort != null) {
                    return producerPort;
                }
			}
		}
        return null;        
    }            
    // ----------------------------------------------------------------------
    void MoveToState(iCS_State newState) {
        myNextState= newState;
        int stackSize= myActiveStack.Count;
        // Determine transition parent node
        myTransitionParent= null;
        iCS_State toTest= newState;
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
    // ----------------------------------------------------------------------
    void UpdateActiveStack() {
        // Update active stack state.
        int stackSize= myActiveStack.Count;
        int stableSize= myQueueIdx+1;
        int newSize= stableSize+myEntrySize;
        if(newSize < stackSize) myActiveStack.RemoveRange(newSize, stackSize-newSize);
        if(newSize > myActiveStack.Capacity) myActiveStack.Capacity= newSize;
        while(myActiveStack.Count < newSize) myActiveStack.Add(null);
        iCS_State toAdd= myNextState;
        for(int offset= myEntrySize-1; offset >= 0; --offset) {
            myActiveStack[stableSize+offset]= toAdd;
            toAdd= toAdd.ParentState;            
        }
        // Prepare to execute entry
        myExecutionState= ExecutionState.RunningEntry;
        myQueueIdx= stableSize;        
    }
    
    // ======================================================================
    // Child Management
    // ----------------------------------------------------------------------
    public void AddChild(SSObject _object) {
        Prelude.choice<iCS_State, iCS_Mux>(_object,
        	(state)=> {
	            if(myChildren.Count == 0) myEntryState= state;
	            myChildren.Add(state);
			},
			(mux)=> {
				myDispatcher.AddChild(mux);
			},
			(otherwise)=> {
			}
		);
    }
    public void RemoveChild(SSObject _object) {
        Prelude.choice<iCS_State, iCS_Mux>(_object,
        	(state)=> {
	            if(state == myEntryState) myEntryState= null;
	            myChildren.Remove(state);
			},
			(mux)=> {
				myDispatcher.RemoveChild(mux);
			},
			(otherwise)=> {
			}
		);
    }
}
