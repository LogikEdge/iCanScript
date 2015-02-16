using UnityEngine;
using System.Collections;
using Subspace;

public class iCS_Transition : SSAction {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    iCS_Package             myTransitionPackage;
    iCS_State               myEndState;
    SSActionWithSignature   myTriggerFunction;
    int                     myTriggerPortIdx;
    bool                    myIsTriggered= false;

    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public iCS_State     EndState    { get { return myEndState; }}
    public bool          DidTrigger  { get { return myIsTriggered; }}
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Transition(string name, SSObject parent, iCS_State endState, iCS_Package transitionPackage, SSActionWithSignature triggerFunc, int portIdx, int priority)
    : base(name, parent, priority) {
        myTransitionPackage= transitionPackage;
        myEndState         = endState;
        myTriggerPortIdx   = portIdx;
        myTriggerFunction  = triggerFunc;
    }
    
    // ======================================================================
    // Update
    // ----------------------------------------------------------------------
    public override void Execute() {
        if(!IsActive) return;
        myIsTriggered= false;
        if(myTransitionPackage != null && myTriggerFunction != null) {
            myTransitionPackage.Execute();            
            if(!myTriggerFunction.IsCurrent) {
                IsStalled= myTransitionPackage.IsStalled;
                return;
            }
            myIsTriggered= (bool)myTriggerFunction[myTriggerPortIdx];
        }
        MarkAsExecuted();
    }
    // ----------------------------------------------------------------------
    public override Connection GetStalledProducerPort() {
        if(IsCurrent) return null;
        return myTransitionPackage.GetStalledProducerPort();
    }
    // ----------------------------------------------------------------------
    public override void ForceExecute() {
        myIsTriggered= false;
        if(myTransitionPackage != null && myTriggerFunction != null) {
            myTransitionPackage.ForceExecute();            
            if(!myTransitionPackage.IsCurrent) {
                IsStalled= myTransitionPackage.IsStalled;
                return;
            }
            myIsTriggered= (bool)myTriggerFunction[myTriggerPortIdx];
        }
        MarkAsExecuted();
    }
}
