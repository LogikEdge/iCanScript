using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class WD_Node : WD_Aggregate {
    // ======================================================================
    // OBJECT LIFETIME MANAGEMENT
    // ----------------------------------------------------------------------
    protected override void Init(string _name, WD_Aggregate _parent) {
        base.Init(_name, _parent);
        
        // Create ports for each field taged with InPort or OutPort.
        foreach(var field in GetInputFields()) {
            WD_InDataPort.CreateInstance(field.Name, this);                                
        }
        foreach(var field in GetOutputFields()) {
            WD_OutDataPort.CreateInstance(field.Name, this);            
        }
    }
    
    // ======================================================================
    // LAYOUT FUNCTIONS
    // ----------------------------------------------------------------------
//    // Moves the node without changing its size.
//    public void SetInitialPosition(Vector2 _initialPosition) {
//        LocalPosition.x= _initialPosition.x - Position.x;
//        LocalPosition.y= _initialPosition.y - Position.y;
//        IsEditorDirty= true;
//    }
//
}
