using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Subspace;

public sealed class iCS_State : SSObject {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public iCS_State             myEntryState    = null;
    public SSAction              myOnEntryAction = null;
    public SSAction              myOnUpdateAction= null;
    public SSAction              myOnExitAction  = null;
    public iCS_State             myParentState   = null;
    public List<iCS_State>       myChildren      = new List<iCS_State>();
           iCS_VerifyTransitions myTransitions   = null;
           
    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public iCS_State             ParentState    { get { return myParentState; } }
    public iCS_State             EntryState     { get { return myEntryState; }     set { myEntryState= value; }}
    public SSAction              OnEntryAction  { get { return myOnEntryAction; }  set { myOnEntryAction= value; }}
    public SSAction              OnUpdateAction { get { return myOnUpdateAction; } set { myOnUpdateAction= value; }}
    public SSAction              OnExitAction   { get { return myOnExitAction; }   set { myOnExitAction= value; }}
    public iCS_VerifyTransitions Transitions    { get { return myTransitions; }}
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_State(int instanceId, string name, iCS_VisualScriptImp visualScript)
    : base(instanceId, name, visualScript) {
        myTransitions= new iCS_VerifyTransitions(instanceId, name, visualScript, instanceId);
    }
    
    // ======================================================================
    // Update
    // ----------------------------------------------------------------------
    public void OnEntry(int runId) {
        if(myOnEntryAction != null) {
            myOnEntryAction.Execute(runId);
        }
    }
    public void OnUpdate(int runId) {
        if(myOnUpdateAction != null) {
            myOnUpdateAction.Execute(runId);
        }
    }
    public void OnExit(int runId) {
        if(myOnExitAction != null) {
            myOnExitAction.Execute(runId);
        }
    }

    // ======================================================================
    // Child Management
    // ----------------------------------------------------------------------
    public void AddChild(SSObject _object) {
        Prelude.choice<iCS_State, iCS_Transition, iCS_Package>(_object,
            (state)=> {
                state.myParentState= this;
                myChildren.Add(state);
            },
            (transition)=> {
                myTransitions.AddChild(transition);
            },
            (package)=> {
                if(package.Name == iCS_Strings.OnEntry) {
                    myOnEntryAction= package;
                }
                else if(package.Name == iCS_Strings.OnUpdate) {
                    myOnUpdateAction= package;
                }
                else if(package.Name == iCS_Strings.OnExit) {
                    myOnExitAction= package;
                }
                else {
    				if(_object.EngineObject.IsTransitionPackage) return;
                    Debug.LogWarning("iCanScript: Only OnEntry, OnUpdate, and OnExit modules can be added to a iCS_State");
                }
            },
            (otherwise)=> {
                Debug.LogWarning("iCanScript: Invalid child type "+_object.TypeName+" being added to state "+Name);
            }
        );
    }
    public void RemoveChild(SSObject _object) {
        Prelude.choice<iCS_State, iCS_Transition, iCS_Package>(_object,
            (state)=> {
                if(state == myEntryState) myEntryState= null;
                myChildren.Remove(state);
            },
            (transition)=> {
                myTransitions.RemoveChild(transition);
            },
            (package)=> {
                if(package == myOnEntryAction) {
                    myOnEntryAction= null;
                }
                else if(package == myOnUpdateAction) {
                    myOnUpdateAction= null;
                }
                else if(package == myOnExitAction) {
                    myOnExitAction= null;
                }
                else {
    				if(_object.EngineObject.IsTransitionPackage) return;
                    Debug.LogWarning("iCanScript: Only OnEntry, OnUpdate, and OnExit modules can be removed from a iCS_State");
                }
            },
            (otherwise)=> {
                Debug.LogWarning("iCanScript: Invalid child type "+_object.TypeName+" being removed from state "+Name);
            }
        );
    }
}
