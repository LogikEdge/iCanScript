using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public abstract class WD_Object : WD_ObjectUtil, IEnumerable<WD_Object> {
    // ======================================================================
    // ATTRIBUTES
    // ----------------------------------------------------------------------
    [SerializeField]    private WD_Aggregate    myParent   = null;
                        public  int             InstanceId = -1;
                        public  WD_Top          Top        = null;
                        
    // ======================================================================
    // CREATION UTILITIES
    // ----------------------------------------------------------------------
    public static DERIVED CreateInstance<DERIVED>(string _name, WD_Aggregate _parent) where DERIVED : WD_Object, new() {
        DERIVED instance= CreateInstance<DERIVED>();
        instance.Init(_name, _parent);
        return instance;
    }
    // ----------------------------------------------------------------------
    protected virtual void Init(string _name, WD_Aggregate _parent) {
        Name= _name;
        Parent= _parent;
        // Add to the save list.
        Case<WD_RootNode, WD_Top, WD_Object>(
            (root) => { root.Graph.AddObject(this); },
            (top)  => { top.RootNode.Graph.AddObject(this); },
            (obj)  => { obj.Top.RootNode.Graph.AddObject(this); }
        );
    }
    // ----------------------------------------------------------------------
    // Control removal of the object (as opposed to the automatic
    // deallocation from a level shutdown).
    public virtual void Dealloc() {
        // Remove from the save list
        Case<WD_RootNode, WD_Top, WD_Object>(
            (root) => { root.Graph.RemoveObject(this); },
            (top)  => { top.RootNode.Graph.RemoveObject(this); },
            (obj)  => { obj.Top.RootNode.Graph.RemoveObject(this); }
        );

        Parent= null;
#if UNITY_EDITOR
        DestroyImmediate(this);
#else
        Destroy(this);
#endif
    }

    // ----------------------------------------------------------------------
    // NAME & TYPE NAME
    // ----------------------------------------------------------------------
    public bool IsNameVisible { get { return name != null && name.Length != 0 && name[0] != WD_EditorConfig.PrivateStringPrefix; }}
    public string Name {
        get {
            if(name == null || name == "") return null;
            return name[0] == WD_EditorConfig.PrivateStringPrefix ? name.Substring(1) : name;
        }
        set { name= value; }
    }
    public string TypeName {
        get { return GetType().Name.Substring(WD_EditorConfig.TypePrefix.Length); }
    }
    public string NameOrTypeName {
        get {
            string displayName= Name;
            return (displayName == null) ? (":"+TypeName) : displayName;
        }
    }
    
    // ======================================================================
    // CHILD MANAGEMENT
    // ----------------------------------------------------------------------
    public WD_Aggregate Parent {
        get { return myParent; }
        set {
            if(myParent != null && myParent != value) {
                myParent.RemoveChild(this);
            }
            if(value != null) {
                value.AddChild(this);
                Top= value.Top;
            }
            myParent= value;
        }
    }
    // ----------------------------------------------------------------------
    // Returns "true" if this node is a child of the given node.
    public bool IsChildOf(WD_Aggregate _parent) {
        if(Parent == _parent) return true;
        return Parent == null ? false : Parent.IsChildOf(_parent);
    }
    public virtual void AddChild(WD_Object _object)     {}
    public virtual void RemoveChild(WD_Object _object)  {}
    private class Enumerator : IEnumerator<WD_Object> {
        public void Reset()                 {}
        public bool MoveNext()              { return false; }
        public WD_Object Current            { get { return null; }}
               object IEnumerator.Current   { get { return Current; }}
               void   System.IDisposable.Dispose() {}
    }
    public virtual IEnumerator<WD_Object>   GetEnumerator()                         { return new Enumerator(); }
                   IEnumerator<WD_Object>   IEnumerable<WD_Object>.GetEnumerator()  { return GetEnumerator(); }
                   IEnumerator              IEnumerable.GetEnumerator()             { return GetEnumerator(); }

    // ======================================================================
    // ITERATION UTILITIES
    // ----------------------------------------------------------------------
    public void ForEachChild(System.Action<WD_Object> fnc) {
        foreach(var child in this) { fnc(child); }
    }
    // ----------------------------------------------------------------------
    public void ForEachChild<T>(System.Action<T> fnc) where T : WD_Object {
        foreach(var child in this) { child.ExecuteIf<T>( (obj)=> { fnc(obj);} ); }
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursiveDepthLast(System.Action<WD_Object> fnc) {
        fnc(this);
        foreach(var child in this) { child.ForEachRecursiveDepthLast(fnc); }
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursiveDepthFirst(System.Action<WD_Object> fnc) {
        foreach(var child in this) { child.ForEachRecursiveDepthFirst(fnc); }
        fnc(this);
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursive(System.Action<WD_Object> fnc) {
        ForEachRecursiveDepthFirst(fnc);
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursiveDepthLast<T>(System.Action<T> fnc) where T : WD_Object {
        ForEachRecursiveDepthLast( (obj)=> { obj.ExecuteIf<T>( (t)=> { fnc(t); } ); } );
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursiveDepthFirst<T>(System.Action<T> fnc) where T : WD_Object {
        ForEachRecursiveDepthFirst( (obj)=> { obj.ExecuteIf<T>( (t)=> { fnc(t); } ); } );
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursive<T>(System.Action<T> fnc) where T : WD_Object {
        ForEachRecursiveDepthFirst<T>(fnc);
    }
    // ----------------------------------------------------------------------
    public int ChildCount() {
        int count= 0;
        ForEachChild( (child)=> { ++count; });
        return count;
    }
    // ----------------------------------------------------------------------
    public int ChildCount<T>() where T : WD_Object {
        int count= 0;
        ForEachChild<T>( (child)=> { ++count; });
        return count;
    }
    // ----------------------------------------------------------------------
    public int ChildCountRecursive() {
        int count= 0;
        ForEachRecursive( (child)=> { ++count; });
        return count;
    }
    // ----------------------------------------------------------------------
    public int ChildCountRecursive<T>() where T : WD_Object {
        int count= 0;
        ForEachRecursive<T>( (child)=> { ++count; });
        return count;
    }
    
    // ======================================================================
    // UPDATE    
    // ----------------------------------------------------------------------
    public bool             IsValid     { get { return doIsValid(); }}
    protected virtual bool  doIsValid() { return true; }
    
    // ======================================================================
    // GUI
    // ----------------------------------------------------------------------
    public          void Layout() {
        Case<WD_Node, WD_Port>(
            (node) => {
                node.Case<WD_RootNode, WD_Top, WD_Node>(
                    (root) => {},
                    (top)  => {},
                    (nd)   => { nd.Top.RootNode.Graph.EditorObjects.Layout(nd.Top.RootNode.Graph.EditorObjects[nd.InstanceId]); }
                );
            },
            (port) => { port.Top.RootNode.Graph.EditorObjects.Layout(port.Top.RootNode.Graph.EditorObjects[port.InstanceId]); }
        );
        IsEditorDirty= false;
        Case<WD_RootNode, WD_Top, WD_Object>(
            (root) => { root.Graph.ReplaceObject(this); },
            (top)  => { top.RootNode.Graph.ReplaceObject(this); },
            (obj)  => { obj.Top.RootNode.Graph.ReplaceObject(this); }
        );
    }
    
    // ----------------------------------------------------------------------
    public bool IsEditorDirty {
        get {
            bool value= true;
            Case<WD_RootNode, WD_Top, WD_Object>(
                (root) => { value= root.Graph.EditorObjects[InstanceId].IsDirty; },
                (top)  => { value= top.RootNode.Graph.EditorObjects[InstanceId].IsDirty; },
                (obj)  => { value= obj.Top.RootNode.Graph.EditorObjects[InstanceId].IsDirty; }
            );
            return value;
        }
        set {
            Case<WD_RootNode, WD_Top, WD_Object>(
                (root) => { root.Graph.EditorObjects[InstanceId].IsDirty= value; },
                (top)  => { top.RootNode.Graph.EditorObjects[InstanceId].IsDirty= value; },
                (obj)  => { obj.Top.RootNode.Graph.EditorObjects[InstanceId].IsDirty= value; }
            );
        }
    }
//    // ----------------------------------------------------------------------
//    public bool IsVisible {
//        get {
//            bool value= true;
//            Case<WD_RootNode, WD_Top, WD_Object>(
//                (root) => { value= false; },
//                (top)  => { value= false; },
//                (obj)  => { value= obj.Top.RootNode.Graph.EditorObjects[InstanceId].IsVisible; }
//            );
//            return value;
//        }
//        set {
//            Case<WD_RootNode, WD_Top, WD_Object>(
//                (root) => { root.Graph.EditorObjects[InstanceId].IsVisible= false; },
//                (top)  => { top.RootNode.Graph.EditorObjects[InstanceId].IsVisible= false; },
//                (obj)  => { obj.Top.RootNode.Graph.EditorObjects[InstanceId].IsVisible= value; }
//            );
//        }
//    }
}
