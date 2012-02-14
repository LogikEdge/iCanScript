using UnityEngine;
using System;
using System.Reflection;
using System.Collections;


public class iCS_ReflectionDesc {
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    public iCS_ObjectTypeEnum   ObjectType = iCS_ObjectTypeEnum.Unknown;
    public string               Company    = "(no company)";
    public string               Package    = "(no package)";
    public string               DisplayName= null;
    public Type                 ClassType  = null;
    public MethodBase           Method = null;
    public FieldInfo            Field= null;
    public bool                 IsGetFieldFlag= true;
    public string               ToolTip= null;
    public string               IconPath= null;
	public string[]				ParamNames= null;
	public Type[]				ParamTypes= null;
	public bool[]				ParamIsOuts= null;
	public string				ReturnName= null;
	public object[]				ParamInitialValues= null;


    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_ReflectionDesc(string company, string package, string name,
                              string toolTip, string iconPath,
                              iCS_ObjectTypeEnum objType, Type classType, MethodBase methodBase, FieldInfo fieldInfo,
                              bool[] paramIsOuts, string[] paramNames, Type[] paramTypes, object[] paramDefaultValues,
                              string returnName) {
        // Editor object information.
		ObjectType        = objType;
        Company           = company;
        Package           = package;
		DisplayName       = name;
		ClassType         = classType;
		Method            = methodBase;
		Field             = fieldInfo;
		IsGetFieldFlag    = fieldInfo != null ? (paramTypes.Length == 0) : true;
        ToolTip           = toolTip;
        IconPath          = iconPath;
		ParamNames        = paramNames;
		ParamTypes        = paramTypes;
		ParamIsOuts       = paramIsOuts;
		ReturnName        = returnName;
		ParamInitialValues= paramDefaultValues;
    }
    // ----------------------------------------------------------------------
    public override string ToString() {
        return FunctionPath+"/"+FunctionSignature;
    }
    

    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public bool IsMethod              { get { return Method != null; }}
    public bool IsField               { get { return Field != null; }}
    public bool IsInstanceField       { get { return ObjectType == iCS_ObjectTypeEnum.InstanceField; }}
    public bool IsStaticField         { get { return ObjectType == iCS_ObjectTypeEnum.StaticField; }}
    public bool IsGetField            { get { return IsField && IsGetFieldFlag; }}
    public bool IsSetField            { get { return IsField && !IsGetFieldFlag; }}
    public bool IsGetInstanceField    { get { return IsInstanceField && IsGetField; }}
    public bool IsSetInstanceField    { get { return IsInstanceField && IsSetField; }}
    public bool IsGetStaticField      { get { return IsStaticField && IsGetField; }}
    public bool IsSetStaticField      { get { return IsStaticField && IsSetField; }}
    public bool IsProperty            { get { return IsGetProperty || IsSetProperty; }}
    public bool IsGetProperty         { get { return IsMethod && ParamTypes.Length == 0 && DisplayName.StartsWith("get_"); }}
    public bool IsSetProperty         { get { return IsMethod && ParamTypes.Length == 1 && DisplayName.StartsWith("set_"); }}
    public bool IsGetInstanceProperty { get { return ObjectType == iCS_ObjectTypeEnum.InstanceMethod && IsGetProperty; }}
    public bool IsSetInstanceProperty { get { return ObjectType == iCS_ObjectTypeEnum.InstanceMethod && IsSetProperty; }}
    public bool IsGetStaticProperty   { get { return ObjectType == iCS_ObjectTypeEnum.StaticMethod && IsGetProperty; }}
    public bool IsSetStaticProperty   { get { return ObjectType == iCS_ObjectTypeEnum.StaticMethod && IsSetProperty; }}
    // ----------------------------------------------------------------------
    public string FieldName    { get { return DisplayName.Substring(4); }}
    public string PropertyName { get { return DisplayName.Substring(4); }}    
    // ----------------------------------------------------------------------
    public string MethodName {
        get {
            if(Method != null) return Method.Name;
            if(Field != null) return Field.Name;
            return null;
        }
    }
    // ----------------------------------------------------------------------
    public Type ReturnType {
        get {
            if(Method != null) {
                if(Method.IsConstructor) return ClassType;
                MethodInfo methodInfo= Method as MethodInfo;
                if(methodInfo == null) return typeof(void);
                return methodInfo.ReturnType;
            }
            if(Field != null) {
                return IsGetField ? Field.FieldType : typeof(void);
            }
            return typeof(void);                    
        }
    }
    // ----------------------------------------------------------------------
    public string FunctionSignature {
        get {
			switch(ObjectType) {
				case iCS_ObjectTypeEnum.Conversion: {
					return DisplayName;
				}
				default: {
		            string signature= DisplayName;
					// Build input string
					string inputStr= "";
		            if(ObjectType == iCS_ObjectTypeEnum.InstanceMethod) {
		                inputStr+= "this"/*+":"+TypeName(ClassType)*/+", ";
		            }
		            for(int i= 0; i < ParamNames.Length; ++i) {
						if(!ParamTypes[i].IsByRef) {
			                inputStr+= ParamNames[i]/*+":"+TypeName(ParamTypes[i])*/+", ";
						}
		            }
					// Add inputs to signature.
					if(inputStr != "") {
			            signature+= " ("+inputStr.Substring(0, inputStr.Length-2)+")";						
					}
					// Build output string
					int nbOfOutputs= 0;
					string outputStr= "";
		            for(int i= 0; i < ParamNames.Length; ++i) {
						if(ParamTypes[i].IsByRef) {
			                outputStr+= ParamNames[i]/*+":"+TypeName(ParamTypes[i].GetElementType())*/+", ";
							++nbOfOutputs;
						}
		            }
					if(ReturnType != null && ReturnType != typeof(void)) {
						++nbOfOutputs;
						if(ReturnName != null && ReturnName != "" && ReturnName != "out") {
							outputStr+= /*" "+*/ReturnName;
						} else {
							outputStr+= ":"+TypeName(ReturnType);
						}
						outputStr+= ", ";
					}
					// Add output to signature.
					if(nbOfOutputs == 1) {
						signature+="->"+outputStr.Substring(0, outputStr.Length-2);
					}
					if(nbOfOutputs > 1) {
						signature+="->("+outputStr.Substring(0, outputStr.Length-2)+")";
					}
					return signature;
				}
			}
        }
    }
    // ----------------------------------------------------------------------
    public string FunctionSignatureNoThis {
        get {
			switch(ObjectType) {
				case iCS_ObjectTypeEnum.Conversion: {
					return DisplayName;
				}
				default: {
		            string signature= DisplayName;
					// Build input string
					string inputStr= "";
		            for(int i= 0; i < ParamNames.Length; ++i) {
						if(!ParamTypes[i].IsByRef) {
			                inputStr+= ParamNames[i]+":"+TypeName(ParamTypes[i])+", ";
						}
		            }
					// Add inputs to signature.
					if(inputStr != "") {
			            signature+= "("+inputStr.Substring(0, inputStr.Length-2)+")";						
					} else {
					    signature+= "()";
					}
					// Build output string
					int nbOfOutputs= 0;
					string outputStr= "";
		            for(int i= 0; i < ParamNames.Length; ++i) {
						if(ParamTypes[i].IsByRef) {
			                outputStr+= ParamNames[i]+":"+TypeName(ParamTypes[i].GetElementType())+", ";
							++nbOfOutputs;
						}
		            }
					if(ReturnType != null && ReturnType != typeof(void)) {
						++nbOfOutputs;
						if(ReturnName != null && ReturnName != "" && ReturnName != "out") {
							outputStr+= /*" "+*/ReturnName;
						} else {
							outputStr+= ":"+TypeName(ReturnType);
						}
						outputStr+= ", ";
					}
					// Add output to signature.
					if(nbOfOutputs == 1) {
						signature+="->"+outputStr.Substring(0, outputStr.Length-2);
					}
					if(nbOfOutputs > 1) {
						signature+="->("+outputStr.Substring(0, outputStr.Length-2)+")";
					}
					return signature;
				}
			}
        }
    }
    // ----------------------------------------------------------------------
    // Returns the function name in the form of "company/package/displayName".
    public string FunctionPath {
        get {
            string package= Package ?? "";
            if(Company == null) return package;
            return Company+"/"+package;
        }
    }
    // ----------------------------------------------------------------------
    // Returns the function name in the form of "company/package/displayName".
    public string FunctionName {
        get {
            string path= FunctionPath;
            if(path == "") return DisplayName;
            return FunctionPath+"/"+DisplayName;
        }
    }

    // ======================================================================
    // Utilities
    // ----------------------------------------------------------------------
    static string TypeName(Type type) {
        return iCS_Types.TypeName(type);
    }
    
}
