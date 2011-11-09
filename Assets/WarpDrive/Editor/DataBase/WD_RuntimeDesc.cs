using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class WD_RuntimeDesc {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    public int                  Id;
    public WD_ObjectTypeEnum    ObjectType;
    public string               Company;
    public string               Package;
    public string               Name;
    public Type                 ClassType;
    public string               MethodName;
    public string[]             ParamNames;
    public Type[]               ParamTypes;
    public bool[]               ParamIsOuts;
    public object[]             ParamDefaultValues;
    public string               ReturnName;
    public Type                 ReturnType;
    
    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public MethodInfo Method {
        get {
            return MethodName != null ? ClassType.GetMethod(MethodName, ParamTypes) : null;            
        }
    }

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public WD_RuntimeDesc() {}
    // ----------------------------------------------------------------------
    // Decodes the string into its constituants.
    public WD_RuntimeDesc(string encoded) {
        Decode(encoded);
    }


    // ======================================================================
    // Archiving
    // ----------------------------------------------------------------------
    // Encode the runtime descriptor into a string.
    // Format: ObjectType:company:package:classType:methodName<[out] paramName[:=defaultValue]:paramType; ...>
    public string Encode(int id) {
        string result= WD_Archive.Encode(id)+":"+WD_Archive.Encode(ObjectType)+":"+Company+":"+Package+":"+Name+":"+WD_Archive.Encode(ClassType)+":"+MethodName+"<";
        for(int i= 0; i < ParamTypes.Length; ++i) {
            if(ParamIsOuts[i]) result+= "out ";
            result+= ParamNames[i];
            if(ParamDefaultValues[i] != null) {
                result+= ":="+WD_Archive.Encode(ParamDefaultValues[i]);
            }
            result+= ":"+WD_Archive.Encode(ParamTypes[i]);
            if(i != ParamTypes.Length-1) result+= ";";
        }
        if(ReturnType != null) {
            result+= ";ret "+(ReturnName != null ? ReturnName : "out")+":"+WD_Archive.Encode(ReturnType);
        }
        result+=">{}";
        return result;
    }
    // ----------------------------------------------------------------------
    // Fills the runtime descriptor from an encoded string.
    public WD_RuntimeDesc Decode(string encoded) {
        // object id
        int end= encoded.IndexOf(':');
        Id= WD_Archive.Decode<int>(encoded.Substring(0, end));
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // object type
        end= encoded.IndexOf(':');
        string objectTypeStr= encoded.Substring(0, end);
        ObjectType= WD_Archive.Decode<WD_ObjectTypeEnum>(objectTypeStr);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // company
        end= encoded.IndexOf(':');
        Company= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // package
        end= encoded.IndexOf(':');
        Package= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // name
        end= encoded.IndexOf(':');
        Name= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // class type
        end= encoded.IndexOf(':');
        string className= encoded.Substring(0, end);
        ClassType= WD_Archive.Decode<Type>(className);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // name
        end= encoded.IndexOf('<');
        MethodName= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        // parameters
        end= encoded.IndexOf('>');
        string parameterString= encoded.Substring(0, end);
        encoded= encoded.Substring(end+1, encoded.Length-end-1);
        ParseParameters(parameterString);
        return this;
    }
    // ----------------------------------------------------------------------
    // Extracts the type of the parameters from the given string.
    void ParseParameters(string paramStr) {
        ReturnType= null;
        List<bool>      paramIsOut   = new List<bool>();
        List<Type>      paramTypes   = new List<Type>();
        List<string>    paramNames   = new List<string>();
        List<object>    paramDefaults= new List<object>();
        while(paramStr.Length > 0) {
            // Return type
            int end= -1;
            if(paramStr.StartsWith("ret ")) {
                end= paramStr.IndexOf(':');
                ReturnName= paramStr.Substring(4, end-4);
                paramStr= paramStr.Substring(end+1, paramStr.Length-end-1);
                end= paramStr.IndexOf(';');
                ReturnType= WD_Archive.Decode<Type>(paramStr.Substring(0, end > 0 ? end : paramStr.Length));
                paramStr= end > 0 ? paramStr.Substring(end+1, paramStr.Length-end-1) : "";
                continue;
            }
            // in/out parameter type
            if(paramStr.StartsWith("out ")) {
                paramIsOut.Add(true);
                paramStr= paramStr.Substring(4, paramStr.Length-4);
            } else {
                paramIsOut.Add(false);
            }                
            // parameter name
            end= paramStr.IndexOf(':');
            paramNames.Add(paramStr.Substring(0, end));
            paramStr= paramStr.Substring(end+1, paramStr.Length-end-1);
            // parameter default value (part 1)
            string defaultValueStr= null;
            if(paramStr.StartsWith("=")) {
                end= paramStr.IndexOf(':');
                defaultValueStr= paramStr.Substring(1, end-1);
                paramStr= paramStr.Substring(end+1, paramStr.Length-end-1);                
            }
            // parameter type.
            end= paramStr.IndexOf(';');
            Type paramType= WD_Archive.Decode<Type>(paramStr.Substring(0, end > 0 ? end : paramStr.Length));
            paramTypes.Add(paramType);
            paramStr= end > 0 ? paramStr.Substring(end+1, paramStr.Length-end-1) : "";
            // parameter default value (part 2)
            if(defaultValueStr != null) {
                paramDefaults.Add(WD_Archive.Decode(defaultValueStr, paramType));
            } else {
                paramDefaults.Add(null);                
            }
        }
        ParamIsOuts= paramIsOut.ToArray();
        ParamTypes = paramTypes.ToArray();
        ParamNames = paramNames.ToArray();
        ParamDefaultValues= paramDefaults.ToArray();
    }

}
