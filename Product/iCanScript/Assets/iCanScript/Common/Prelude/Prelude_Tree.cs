using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static partial class Prelude {
	public class Tree<T> where T : class {
	    // =================================================================================
	    // Fields
	    // ---------------------------------------------------------------------------------
		T				myValue   = default(T);
        Tree<T>         myParent  = null;
		List<Tree<T>>	myChildren= null;
		
	    // =================================================================================
	    // Properties
	    // ---------------------------------------------------------------------------------
		public T 		     Value		{ get { return myValue; } set { myValue= value; }}
		public List<Tree<T>> Children	{ get { return myChildren; }}

	    // =================================================================================
	    // Initialization
	    // ---------------------------------------------------------------------------------
		public Tree(T value) { myValue= value; }

	    // =================================================================================
	    // Child management.
	    // ---------------------------------------------------------------------------------
		public void AddChild(Tree<T> node) {
			if(myChildren == null) myChildren= new List<Tree<T>>();
            node.myParent= this;
			myChildren.Add(node);			
		}
		public void AddChild(T childValue)	{
			AddChild(new Tree<T>(childValue));
		}
		// This method exist to bypass bug in MonoDev compiler.
		public void AddChild(System.Object child) {
			var tree = child as Tree<T>;
			if (tree != null) {
				AddChild (tree);
			} else {
				var t = child as T;
				if (t != null) {
					AddChild (t);
				}
				else {
					Debug.Log ("Prelude.Tree<T>: Unable to determine type to add");
				}
			}
		}
		public bool RemoveChild(T childValue) {
			if(myChildren == null) return false;
			int idx= myChildren.FindIndex(x=> x.Value.Equals(childValue));
			if(idx < 0) return false;
			myChildren.RemoveAt(idx);
			return true;
		}
		public void Sort(Func<T,T,int> criteria) {
			if(myChildren == null) return;
			myChildren.Sort((x,y)=> criteria(x.Value, y.Value));
		}
	}
}

