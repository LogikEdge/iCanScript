using UnityEngine;
using UnityEditor;
using System.Collections;

public class AP_MenuBoolean {

    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/CBoolean")]
    public static void AddNodeCBoolean(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_CBoolean function= AP_CBoolean.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/CBoolean", true)]
    public static bool ValidateAddNodeCBoolean(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }
    
    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Not")]
    public static void AddNodeNot(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_Not function= AP_Not.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Not", true)]
    public static bool ValidateAddNodeNot(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }
    
    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/And")]
    public static void AddNodeAnd(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_And function= AP_And.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/And", true)]
    public static bool ValidateAddNodeAnd(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }
    
    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Or")]
    public static void AddNodeOr(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_Or function= AP_Or.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Or", true)]
    public static bool ValidateAddNodeOr(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }
    
    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Xor")]
    public static void AddNodeXor(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_Xor function= AP_Xor.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Xor", true)]
    public static bool ValidateAddNodeXor(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }

    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Decoder2")]
    public static void AddNodeDecoder2(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_Decoder2 function= AP_Decoder2.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Decoder2", true)]
    public static bool ValidateAddNodeDecoder2(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }

    // ----------------------------------------------------------------------
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Decoder4")]
    public static void AddNodeDecoder4(MenuCommand command) {
        AP_MenuContext context= command.context as AP_MenuContext;
        AP_Node parent= context.SelectedObject as AP_Node;
        AP_Decoder4 function= AP_Decoder4.CreateInstance("", parent);
        function.SetInitialPosition(context.GraphPosition);        
    }
    [MenuItem("CONTEXT/AnimationPro/Edit/Module/Logic/Decoder4", true)]
    public static bool ValidateAddNodeDecoder4(MenuCommand command) {
        return AP_MenuUtilities.ValidateAddNode(command);
    }

}
