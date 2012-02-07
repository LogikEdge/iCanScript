using UnityEngine;

public static class iCS_Installer {
    // ---------------------------------------------------------------
    // Indicates if the installation  has already been executed.
    static bool IsInstalled= false;
    
    // ------------------------------------------------------------------------
    // Use this function to install in bulk iCanScript nodes from your classes.
    // All public fields, properties, and functions will become available when
    // installing in bulk.  You can use the Unity or .NET installation files
    // as a template for your own code (see iCS_NETClasses & iCS_UnityClasses).
    //
    // You should consider using the iCanScript custom attributes fro fine
    // grain control over the installation of your nodes (preferred appraoch).
    // iCanScript will automatically extract from the assembly your nodes
    // when custom attributes are used (you do not need to manually add the
    // calsses).
    //
    // Currently, we install the Unity & .NET components in the node database.
    static iCS_Installer() {
        if(IsInstalled) return;
        IsInstalled= true;
        
        iCS_NETClasses.PopulateDataBase();
        iCS_UnityClasses.PopulateDataBase();        
    }

    // ------------------------------------------------------------------------
    // Forces invokation of the constructor if not already done.
    public static void Install() {}
}
