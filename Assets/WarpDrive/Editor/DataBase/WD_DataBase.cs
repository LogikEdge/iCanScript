using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class WD_DataBase {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    public static List<WD_BaseDesc>    Functions  = new List<WD_BaseDesc>();
    
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
    // Returns the descriptor associated with the given company/package/function.
    public static WD_BaseDesc GetDescriptor(string company, string package, string function) {
        foreach(var desc in Functions) {
            if(desc.Company == company &&
               desc.Package == package &&
               desc.Name    == function) return desc;
        }
        return null;
    }
    // ----------------------------------------------------------------------
    // Finds a conversion that matches the given from/to types.
    public static WD_ConversionDesc FindConversion(Type fromType, Type toType) {
        foreach(var desc in Functions) {
            if(desc is WD_ConversionDesc) {
                WD_ConversionDesc conv= desc as WD_ConversionDesc;
                if(WD_Types.CanBeConnectedWithoutConversion(fromType, conv.FromType) &&
                   WD_Types.CanBeConnectedWithoutConversion(conv.ToType, toType)) return conv;
            }
        }
        return null;
    }
    // ----------------------------------------------------------------------
    // Returns a string that uniquely describes the descriptor.
    public static string ToString(WD_BaseDesc desc) {
        string result= desc.Company+":"+desc.Package+":"+desc.ClassType.AssemblyQualifiedName+":"+desc.Name+"<";
        if(desc is WD_FunctionDesc) {
            WD_FunctionDesc funcDesc= desc as WD_FunctionDesc;
            foreach(var type in funcDesc.ParameterTypes) {
                result+= type.AssemblyQualifiedName+";";
            }
            result+= funcDesc.ReturnType != null ? funcDesc.ReturnType.AssemblyQualifiedName : typeof(void).ToString();
        } else if(desc is WD_ConversionDesc) {
            WD_ConversionDesc convDesc= desc as WD_ConversionDesc;
            result+= convDesc.FromType.AssemblyQualifiedName+";"+convDesc.ToType.AssemblyQualifiedName;
        }
        return result+">{}";
    }
    // ----------------------------------------------------------------------
    // Returns the BaseDesc associated with the given string.
    public static WD_BaseDesc FromString(string encoded) {
        foreach(var desc in Functions) {
            if(desc.ToString() == encoded) return desc;
        }
        return null;
    }
    // ----------------------------------------------------------------------
    // Decodes the string into its constituants.
    public static void ParseDescriptorString(string encoded, out string company, out string package,
                                             out Type classType, out string name,
                                             out Type[] parameters, out string[] children) {
        // company
        int end= encoded.IndexOf(':');
        company= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // package
        end= encoded.IndexOf(':');
        package= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // class type
        end= encoded.IndexOf(':');
        string className= encoded.Substring(0, end);
        classType= Type.GetType(className);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // name
        end= encoded.IndexOf('<');
        name= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // parameters
        end= encoded.IndexOf('>');
        string parameterString= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        parameters= ParseParameters(parameterString);
        // children
        children= new string[0];
    }
    // ----------------------------------------------------------------------
    // Extracts the type of the parameters from the given string.
    static Type[] ParseParameters(string paramStr) {
        List<Type>  parameters= new List<Type>();
        int end= 0;
        do {
            end= paramStr.IndexOf(';');
            if(end < 0) end= paramStr.Length;
            if(end > 0) {
                parameters.Add(Type.GetType(paramStr.Substring(0, end)));
                if(end != paramStr.Length) {
                    paramStr= paramStr.Substring(end+1, paramStr.Length-end-1);                    
                } else {
                    paramStr= "";
                }
            }
        }
        while(end > 0);
        return parameters.ToArray();
    }
    
    // ======================================================================
    // Container management functions
    // ----------------------------------------------------------------------
    // Removes all previously recorded functions.
    public static void Clear() {
        Functions.Clear();
    }
    // ----------------------------------------------------------------------
    // Adds a conversion function
    public static void AddConversion(string company, string package, Type classType, string icon, MethodInfo methodInfo, Type fromType, Type toType) {
        foreach(var desc in Functions) {
            if(desc is WD_ConversionDesc) {
                WD_ConversionDesc conv= desc as WD_ConversionDesc;
                if(conv.FromType == fromType && conv.ToType == toType) {
                    Debug.LogWarning("Duplicate conversion function from "+fromType+" to "+toType+" exists in classes "+conv.Method.DeclaringType+" and "+methodInfo.DeclaringType);
                    return;
                }                
            }
        }
        Functions.Add(new WD_ConversionDesc(company, package, classType, icon, methodInfo, fromType, toType));
    }
    // ----------------------------------------------------------------------
    // Adds an execution function (no context).
    public static void AddFunction(string company, string package, string classToolTip, Type classType, // Class info
                                   string methodName,                                                   // Function info
                                   string[] paramNames, Type[] paramTypes,                              // Parameter info
                                   bool[] paramInOuts, object[] paramDefaults,
                                   string retName, Type retType,                                        // Return value info
                                   string toolTip, string icon, MethodInfo methodInfo) {
        WD_FunctionDesc fd= new WD_FunctionDesc(company, package, classToolTip, classType,
                                                methodName, retName, retType, toolTip, icon,
                                                paramNames, paramTypes, paramInOuts, paramDefaults,
                                                methodInfo);
        Functions.Add(fd);
    }
    // ----------------------------------------------------------------------
    // Adds a class.
    public static void AddClass(string company, string package, string className, string classToolTip, Type classType, string classIcon,                        // Class info
                                string[] fieldNames, Type[] fieldTypes, bool[] fieldInOuts,                                                                     // Field info
                                string[] propertyNames, Type[] propertyTypes, bool[] propertyInOuts,                                                            // Property info
                                MethodInfo[] methodInfos, string[] methodNames, string[] returnNames, Type[] returnTypes, string[] toolTips, string[] icons,    // Method info
                                string[][] parameterNames, Type[][] parameterTypes, bool[][] parameterInOuts) {                                                 // Method parameter info
        Functions.Add(new WD_ClassDesc(company, package, className, classToolTip, classType, classIcon,
                                       fieldNames, fieldTypes, fieldInOuts,
                                       propertyNames, propertyTypes, propertyInOuts,
                                       methodInfos, methodNames, returnNames, returnTypes, toolTips, icons,
                                       parameterNames, parameterTypes, parameterInOuts));    
    }


}
