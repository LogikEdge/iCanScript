using UnityEngine;
using System.Collections;
using iCanScript.Editor;

public partial class iCS_IStorage {
    // ----------------------------------------------------------------------
    /// Ask each object to perform their own sanity check.
    public void SanityCheck() {
        var kSanityCheckServiceKey= "SanityCheck";
        ErrorController.Clear(kSanityCheckServiceKey);
        // -- Verify visual script attributes --
        if(CodeGenerationConfig.BaseType == null) {
            var message= "Base Type for code generation is invalid. ";
            ErrorController.AddError(kSanityCheckServiceKey, message, VisualScript, 0);
        }
        // -- Ask each object to perform their own sanity check --
        ForEach(o=> o.SanityCheck(kSanityCheckServiceKey));
    }

}
