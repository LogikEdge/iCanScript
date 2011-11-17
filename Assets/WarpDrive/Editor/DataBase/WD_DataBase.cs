using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class WD_DataBase {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public static List<WD_ReflectionDesc>   Functions  = new List<WD_ReflectionDesc>();
    
    // ======================================================================
    // DataBase functionality
    // ----------------------------------------------------------------------
    // Returns all the company names for which a WarpDrive component exists.
    public static string[] GetCompanies() {
        List<string> companies= new List<string>();
        foreach(var func in Functions) {
            WarpDrive.AddUniqu<string>(func.Company, companies);
        }
        return companies.ToArray();
    }
    // ----------------------------------------------------------------------
    // Returns all available packages of the given company.
    public static string[] GetPackages(string company) {
        List<string> packages= new List<string>();
        foreach(var func in Functions) {
            if(func.Company == company) {
                WarpDrive.AddUniqu<string>(func.Package, packages);                
            }
        }
        return packages.ToArray();
    }
    // ----------------------------------------------------------------------
    // Returns all available functions of the given company/package.
    public static string[] GetFunctions(string company, string package) {
        List<string> functions= new List<string>();
        foreach(var func in Functions) {
            if(func.Company == company && func.Package == package) {
                WarpDrive.AddUniqu<string>(func.Name, functions);                
            }
        }
        return functions.ToArray();
    }
    // ----------------------------------------------------------------------
    // Returns all available functions parameters for the given
    // company/package/function.
    public static string[] GetFunctionSignatures(string company, string package, string functionName) {
        List<string> parameters= new List<string>();
        foreach(var func in Functions) {
            if(func.Company == company && func.Package == package && func.Name == functionName) {
                parameters.Add(GetFunctionSignature(func));
            }
        }
        return parameters.ToArray();
    }

    // ----------------------------------------------------------------------
    public static string GetFunctionSignature(WD_ReflectionDesc desc) {
        string signature= TypeName(desc.ReturnType);
        signature+= " "+desc.Name+"(";
        for(int i= 0; i < desc.ParamNames.Length; ++i) {
            signature+= TypeName(desc.RuntimeDesc.ParamTypes[i])+" "+desc.ParamNames[i];
            if(i != desc.ParamNames.Length-1) signature+=", ";
        }
        return signature+")";
    }
    // ----------------------------------------------------------------------
    static string TypeName(Type type) {
        if(type == null) return "void";
        if(type == typeof(float)) return "float";
        return type.Name;
    }
    // ----------------------------------------------------------------------
    // Returns the descriptor associated with the given company/package/function.
    public static WD_ReflectionDesc GetDescriptor(string company, string package, string functionName, string signature) {
        foreach(var desc in Functions) {
            if(desc.Company == company &&
               desc.Package == package &&
               desc.Name    == functionName) {
                   if(signature == null) return desc;
                   if(signature == GetFunctionSignature(desc)) return desc;
               }
        }
        return null;
    }
    // ----------------------------------------------------------------------
    // Finds a conversion that matches the given from/to types.
    public static WD_ReflectionDesc FindConversion(Type fromType, Type toType) {
        foreach(var desc in Functions) {
            if(IsConversion(desc)) {
                WD_RuntimeDesc conv= desc.RuntimeDesc;
                if(WD_Types.CanBeConnectedWithoutConversion(fromType, conv.ParamTypes[0]) &&
                   WD_Types.CanBeConnectedWithoutConversion(conv.ReturnType, toType)) return desc;
            }
        }
        return null;
    }
    // ----------------------------------------------------------------------
    // Returns true if the given desc is a conversion function.
    public static bool IsConversion(WD_ReflectionDesc desc) {
        return desc.RuntimeDesc.ObjectType == WD_ObjectTypeEnum.Conversion;
    }
    
    // ======================================================================
    // Container management functions
    // ----------------------------------------------------------------------
    // Removes all previously recorded functions.
    public static void Clear() {
        Functions.Clear();
    }
    // ----------------------------------------------------------------------
    public static void AddStaticField(FieldInfo field, WD_ParamDirectionEnum direction, Type classType,
                                      string company, string package, string className, string toolTip, string iconPath) {
        
    }
    // ----------------------------------------------------------------------
    public static void AddInstanceField(FieldInfo field, WD_ParamDirectionEnum direction, Type classType,
                                        string company, string package, string className, string toolTip, string iconPath) {
        
    }
    // ----------------------------------------------------------------------
    public static void AddStaticMethod() {
        
    }
    // ----------------------------------------------------------------------
    public static void AddInstanceMethod() {
        
    }
    // ----------------------------------------------------------------------
    public static void AddConversion() {
        
    }
    // ----------------------------------------------------------------------
    // Adds a conversion function
    public static void AddConversion(string company, string package, Type classType, string iconPath, MethodInfo methodInfo, Type fromType, Type toType) {
        // Don't accept automatic conversion if it already exist.
        foreach(var desc in Functions) {
            if(IsConversion(desc)) {
                WD_RuntimeDesc conv= desc.RuntimeDesc;
                if(conv.ParamTypes[0] == fromType && conv.ReturnType == toType) {
                    Debug.LogWarning("Duplicate conversion function from "+fromType+" to "+toType+" exists in classes "+conv.Method.DeclaringType+" and "+methodInfo.DeclaringType);
                    return;
                }                
            }
        }
        Add(company, package, fromType.Name+"->"+toType.Name,
            "Converts from "+fromType.Name+" to "+toType.Name, iconPath,
            WD_ObjectTypeEnum.Conversion, classType, methodInfo,
            new bool[1]{false}, new string[1]{fromType.Name}, new Type[1]{fromType}, new object[1]{null},
            toType.Name, toType);
    }
    // ----------------------------------------------------------------------
    // Adds an execution function (no context).
    public static void AddFunction(string company, string package, string classToolTip, Type classType, // Class info
                                   string name,                                                   // Function info
                                   string[] paramNames, Type[] paramTypes,                              // Parameter info
                                   bool[] paramIsOuts, object[] paramDefaults,
                                   string retName, Type retType,                                        // Return value info
                                   string toolTip, string iconPath, MethodInfo methodInfo) {
        Add(company, package, name,
            toolTip ?? classToolTip, iconPath,
            WD_ObjectTypeEnum.Function, classType, methodInfo,
            paramIsOuts, paramNames, paramTypes, paramDefaults,
            retName, retType);
    }
    // ----------------------------------------------------------------------
    // Adds a new database record.
    public static WD_ReflectionDesc Add(string company, string package, string name,
                                        string toolTip, string iconPath,
                                        WD_ObjectTypeEnum objType, Type classType, MethodInfo methodInfo,
                                        bool[] paramIsOuts, string[] paramNames, Type[] paramTypes, object[] paramDefaults,
                                        string retName, Type retType) {
        WD_ReflectionDesc fd= new WD_ReflectionDesc(company, package, name,
                                                    toolTip, iconPath,
                                                    objType, classType, methodInfo,
                                                    paramIsOuts, paramNames, paramTypes, paramDefaults,
                                                    retName, retType);
        Functions.Add(fd);
        return fd;
    }
    
//    // ----------------------------------------------------------------------
//    // Adds a class.
//    public static void AddClass(string company, string package, string className, string classToolTip, Type classType, string classIcon,                        // Class info
//                                string[] fieldNames, Type[] fieldTypes, bool[] fieldInOuts,                                                                     // Field info
//                                string[] propertyNames, Type[] propertyTypes, bool[] propertyInOuts,                                                            // Property info
//                                MethodInfo[] methodInfos, string[] methodNames, string[] returnNames, Type[] returnTypes, string[] toolTips, string[] icons,    // Method info
//                                string[][] parameterNames, Type[][] parameterTypes, bool[][] parameterInOuts) {                                                 // Method parameter info
//        Functions.Add(new WD_ClassDesc(company, package, className, classToolTip, classType, classIcon,
//                                       fieldNames, fieldTypes, fieldInOuts,
//                                       propertyNames, propertyTypes, propertyInOuts,
//                                       methodInfos, methodNames, returnNames, returnTypes, toolTips, icons,
//                                       parameterNames, parameterTypes, parameterInOuts));    
//    }
//

}
