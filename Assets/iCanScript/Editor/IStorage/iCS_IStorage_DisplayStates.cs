using UnityEngine;
using System.Collections;

public partial class iCS_IStorage {
    public bool IsIconizedOnDisplay(iCS_EditorObject eObj)  { return eObj.IsIconized; }
    public bool IsUnfoldedOnDisplay(iCS_EditorObject eObj)  { return eObj.IsUnfolded; }
    public bool IsFoldedOnDisplay(iCS_EditorObject eObj)    { return eObj.IsFolded; }
    public bool IsVisibleOnDisplay(iCS_EditorObject eObj)   { return eObj.IsVisible; }
    public bool IsIconizedInLayout(iCS_EditorObject eObj)   { return eObj.IsIconized; }
    public bool IsUnfoldedInLayout(iCS_EditorObject eObj)   { return eObj.IsUnfolded; }
    public bool IsFoldedInLayout(iCS_EditorObject eObj)     { return eObj.IsFolded; }
    public bool IsVisibleInLayout(iCS_EditorObject eObj)    { return eObj.IsVisible; }
    
    // ======================================================================
    // Display Options
    // ----------------------------------------------------------------------
    public void Fold(iCS_EditorObject eObj) {
        if(!eObj.IsNode) return;    // Only nodes can be folded.
        eObj.IsDirty= true;
        if(eObj.IsFunction) {
            Unfold(eObj);
            return;
        }
        eObj.Fold();
        ForEachChild(eObj, child=> { if(child.IsPort) child.Unfold(); });
    }
    public void Fold(int id) { if(IsValid(id)) Fold(EditorObjects[id]); }
    // ----------------------------------------------------------------------
    public void Iconize(iCS_EditorObject eObj) {
        if(!eObj.IsNode) return;
        eObj.Iconize();
        ForEachChild(eObj, child=> { if(child.IsPort) child.Iconize(); });
        eObj.IsDirty= true;
        if(IsValid(eObj.ParentId)) {
            eObj.Parent.IsDirty= true;
        }
    }
    public void Iconize(int id) { if(IsValid(id)) Iconize(EditorObjects[id]); }
    // ----------------------------------------------------------------------
    public void Unfold(iCS_EditorObject eObj) {
        if(!eObj.IsNode) return;
        eObj.Unfold();
        ForEachChild(eObj, child=> { if(child.IsPort) child.Unfold(); });
        eObj.IsDirty= true;
    }
    public void Unfold(int id) { if(IsValid(id)) Unfold(EditorObjects[id]); }

}
