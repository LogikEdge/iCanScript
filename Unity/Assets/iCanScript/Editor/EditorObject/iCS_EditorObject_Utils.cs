using UnityEngine;
using System.Collections;
using iCanScript.Editor;

public partial class iCS_EditorObject {
    // =====================================================================
    // Edition Utility
    // ----------------------------------------------------------------------
	public bool CanHavePackageAsParent() {
		if(IsPort || IsBehaviour || IsState || IsMessage || IsOnStatePackage) {
			return false;
		}
		return true;
	}
    // ----------------------------------------------------------------------
    public bool CanBeDeleted() {
        if(IsBehaviour || IsFixDataPort) return false;
        return true;
    }
	
    // =====================================================================
    // Layout utilites
    // ----------------------------------------------------------------------
    // Adds a margin around the given rectangle
    static Rect AddMargins(Rect r) {
        var m= iCS_EditorConfig.MarginSize;
        var m2= 2f*m;
        return new Rect(r.x-m, r.y-m, r.width+m2, r.height+m2);
    }
    // ----------------------------------------------------------------------
    // Returns true if a collision exists in the given node array.
    static bool DoesCollide(iCS_EditorObject[] nodes) {
       var len= nodes.Length;
       Rect[] rs= new Rect[len];
       for(int i= 0; i < len; ++ i) {
           rs[i]= nodes[i].AnimatedRect;
       }
       for(int i= 0; i < len-1; ++i) {
           var r1= AddMargins(rs[i]);
           for(int j= i+1; j < len; ++j) {
               if(Math3D.DoesCollide(r1, rs[j])) {
                   return true;
               }
           }
       }
       return false;
   } 

    // ----------------------------------------------------------------------
    /// Performs a snaity check on the visual script object.
   public void SanityCheck(string serviceKey) {
       // Verify that the runtime portion still exists.
       if(IsKindOfFunction) {
           var memberInfo= iCS_LibraryDatabase.GetAssociatedDescriptor(this);
           if(memberInfo == null) {
               var message= "Unable to find the runtime code for "+FullName;
               ErrorController.AddError(serviceKey, message, IStorage.VisualScript, InstanceId);
           }
       }
   }

}
