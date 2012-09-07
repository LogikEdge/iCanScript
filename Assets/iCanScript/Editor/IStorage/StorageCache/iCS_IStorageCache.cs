using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// --------------------------------------------------------------------------
// An instance of this class is dynamically created from the EditorObjects.
public class iCS_IStorageCache {
    // ======================================================================
    // Child Classes
    // ----------------------------------------------------------------------
    public class iCS_EditorObjectCache {
        public bool         IsValid= false;
        public int          ParentId= -1;
        public List<int>    Children= new List<int>();
        public Rect         DisplayPosition;
        public float        AnimationTime= 0;
		public object		InitialValue= null;
		
        public iCS_EditorObjectCache()  { Reset(); }
        public void Reset() {
            IsValid= false;
            ParentId= -1;
            Children.Clear();
			AnimationTime= 0;
			InitialValue= null;
        }
        public void AddChild(int id, iCS_EditorObjectCache toAdd) {
            if(Prelude.elem(id, Children.ToArray())) return;
            Children.Add(id);
        }
        public void RemoveChild(int id, iCS_EditorObjectCache toDelete) {
            for(int i= 0; i < Children.Count; ++i) {
                if(Children[i] == id) {
                    Children.RemoveAt(i);
                    return;
                }
            }
        }
        public bool AreChildrenInSameOrder(int[] orderedChildren) {
            int i= 0;
            for(int j= 0; j < Children.Count; ++j) {
                if(Children[j] == orderedChildren[i]) {
                    if(++i >= orderedChildren.Length) return true;
                };
            }
            return false;
        }
        public void ReorderChildren(int[] orderedChildren) {
            if(AreChildrenInSameOrder(orderedChildren)) return;
            List<int> others= Prelude.filter(c=> Prelude.notElem(c,orderedChildren), Children);
            int i= 0;
            Prelude.forEach(c=> Children[i++]= c, orderedChildren);
            Prelude.forEach(c=> Children[i++]= c, others);
        }
    }
    
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public List<iCS_EditorObjectCache>   StorageCache= new List<iCS_EditorObjectCache>();

    // ======================================================================
    // Tree Cach Containement Functionality
    // ----------------------------------------------------------------------
    public iCS_EditorObjectCache this[int i] {
        get { return StorageCache[i]; }
    }
    // ----------------------------------------------------------------------
    public bool IsValid(int id)      { return id >= 0 && id < StorageCache.Count && StorageCache[id].IsValid; }
    public bool IsInvalid(int id)    { return !IsValid(id); }
    // ----------------------------------------------------------------------
    public void CreateInstance(iCS_EditorObject obj) {
        if(obj.InstanceId < StorageCache.Count && StorageCache[obj.InstanceId].IsValid) {
            Debug.LogWarning("Trying to create a TreeNode with the same id has an existing TreeNode. (id)=>"+obj.InstanceId);
        }
        // Create slots in the tree cache to hold the new instance.
        while(StorageCache.Count <= obj.InstanceId) StorageCache.Add(new iCS_EditorObjectCache());
        UpdateInstance(obj);
    }
    public void UpdateInstance(iCS_EditorObject obj) {
        int id      = obj.InstanceId;
        int parentId= obj.ParentId;

        // Protect against misuse.
        if(id < 0 || id >= StorageCache.Count) {
            Debug.LogError("Trying to update an invalid TreeNode with id:"+id);
        }
        
        // Remove link to parent if parent has changed.
        var node= StorageCache[id];
        if(node.ParentId != parentId && IsValid(node.ParentId)) {
            StorageCache[node.ParentId].RemoveChild(id, node);
        }
        node.ParentId= parentId;

        // Update the parent if it is present.
        if(IsValid(parentId)) {
            StorageCache[parentId].AddChild(id, node);
        }
        // Scan for already configured children.
        for(int i= 0; i < StorageCache.Count; ++i) {
            if(StorageCache[i].ParentId == id) {
                node.AddChild(i, StorageCache[i]);
            }
        }
        node.IsValid= true;
    }
    // ----------------------------------------------------------------------
    public void DestroyInstance(int id) {
        if(IsInvalid(id)) return;
        
        // Remove from parent.
        var nd= StorageCache[id];
        if(IsValid(nd.ParentId)) {
            StorageCache[nd.ParentId].RemoveChild(id, nd);
        }
        StorageCache[id].Reset();
    }

    // ======================================================================
    // Tree Iterations
    // ----------------------------------------------------------------------
    public void ForEachChild(Action<int> fnc) {
        for(int id= 0; id < StorageCache.Count; ++id) {
            if(IsValid(id) && StorageCache[id].ParentId == -1) fnc(id);
        }
    }
    public bool ForEachChild(Func<int,bool> fnc) {
        for(int id= 0; id < StorageCache.Count; ++id) {
            if(IsValid(id) && StorageCache[id].ParentId == -1) {
                if(fnc(id)) return true;
            }
        }
        return false;
    }
    public void ForEachChild(int id, Action<int> fnc) {
        if(IsInvalid(id)) return;
        var nd= StorageCache[id];
        foreach(var child in nd.Children) {
            if(StorageCache[id].IsValid) fnc(child);
        }
    }
    public bool ForEachChild(int id, Func<int,bool> fnc) {
        if(IsInvalid(id)) return false;
        var nd= StorageCache[id];
        foreach(var child in nd.Children) {
            if(StorageCache[id].IsValid) {
                if(fnc(child)) return true;
            }
        }
        return false;
    }
    // ----------------------------------------------------------------------
    public void ForEachRecursiveDepthFirst(Action<int> fnc) {
        // Nothing to do if the id is invalid
        if(IsInvalid(0)) return;
        // First do all children.
        ForEachChild((child) => { ForEachRecursiveDepthFirst(child, fnc); });
        // ... then root node.
        fnc(0);
    }
    public void ForEachRecursiveDepthFirst(int id, Action<int> fnc) {
        // Nothing to do if the id is invalid.
        if(IsInvalid(id)) return;

        // First iterate through all children ...
        var nd= StorageCache[id];
        foreach(var child in nd.Children) {
            ForEachRecursiveDepthFirst(child, fnc);
        }
        
        // ... then this node.
        fnc(id);
    }
    public void ForEachRecursiveDepthLast(Action<int> fnc) {
        // Nothing to do if the id is invalid
        if(IsInvalid(0)) return;
        // First root node.
        fnc(0);
        // ... then all children.
        ForEachChild((child) => { ForEachRecursiveDepthLast(child, fnc); });
    }
    public void ForEachRecursiveDepthLast(int id, Action<int> fnc) {
        // Nothing to do if the id is invalid
        if(IsInvalid(id)) return;
        // First this node ...
        fnc(id);
        // ... then iterate through all children.
        var nd= StorageCache[id];
        foreach(var child in nd.Children) {
            ForEachRecursiveDepthLast(child, fnc);
        }
    }
    // ----------------------------------------------------------------------
    public void ForEachChildRecursiveDepthFirst(int id, Action<int> fnc) {
        // Nothing to do if the id is invalid.
        if(IsInvalid(id)) return;
        // Don't use the id it is has been removed.
        var nd= StorageCache[id];        
        // Iterate through all children ...
        foreach(var child in nd.Children) {
            ForEachRecursiveDepthFirst(child, fnc);
        }
    }
    public void ForEachChildRecursiveDepthLast(int id, Action<int> fnc) {
        // Nothing to do if the id is invalid
        if(IsInvalid(id)) return;
        // Don't use the id it is has been removed.
        var nd= StorageCache[id];        
        // Iterate through all children.
        foreach(var child in nd.Children) {
            ForEachRecursiveDepthLast(child, fnc);
        }
    }
}
