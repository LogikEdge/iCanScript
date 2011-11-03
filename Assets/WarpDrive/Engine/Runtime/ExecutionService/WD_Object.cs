using UnityEngine;
using System.Collections;

public class WD_Object {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public string Name     { get; set; }
    public string TypeName { get { return GetType().Name; }}
    
    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public WD_Object(string name) { Name= name; }
}
