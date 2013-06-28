using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class iCS_EngineObject {
    // ======================================================================
    // Database Fields
    // ----------------------------------------------------------------------
    public iCS_ObjectTypeEnum    ObjectType         = iCS_ObjectTypeEnum.Unknown;
    public int                   InstanceId         = -1;
    public int                   ParentId           = -1;
    public string                QualifiedType      = "";
    public string                RawName            = "";
	public Vector2				 LocalAnchorPosition= Vector2.zero;
	public float				 Scale              = 1.0f;  // Used for children scale
    public iCS_DisplayOptionEnum DisplayOption      = iCS_DisplayOptionEnum.Unfolded;
    public bool                  IsNameEditable     = true;

	// Node specific attributes ---------------------------------------------
	public string				 MethodName       = null;
	public int					 NbOfParams       = 0;     // Also used for port group
    public string                IconGUID         = null;
    public string                Tooltip          = null;
    public int                   ExecutionPriority= 0;
	public int					 LayoutPriority   = 0;

    // Port specific attributes ---------------------------------------------
    public int                   SourceId           = -1;
    public int                   PortIndex          = -1;
	public string				 InitialValueArchive= null;

    // State specific attributes ---------------------------------------------
    public bool                  IsEntryState= false;

    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public bool   IsValid         { get { return InstanceId != -1; }}
    public bool   IsParentValid   { get { return ParentId != -1; }}
    public bool   IsSourceValid   { get { return SourceId != -1; }}
    public bool   IsNameEmpty     { get { return RawName == null || RawName == ""; }}
    public string TypeName        { get { return iCS_Types.TypeName(RuntimeType);}} 
    public Type   RuntimeType     {
        get {
            bool conversionPerformed= false;
            var ty= iCS_Types.TypeFromAssemblyQualifiedName(QualifiedType, out conversionPerformed);
            if(conversionPerformed) {
                QualifiedType= ty.AssemblyQualifiedName;
            }
            return ty;
        }
    }
    public string Name {
        get { return IsNameEmpty ? ":"+TypeName : RawName; }
        set { RawName= value; }
    }
    // Port Specific accesors ------------------------------------------------
    public int PortGroup {
        get { return NbOfParams; }
        set { NbOfParams= value; }
    }
    public iCS_EdgeEnum Edge {
        get {
            var edge= LocalAnchorPosition.x;
            if(Math3D.IsEqual(edge, 1f)) return iCS_EdgeEnum.Left;
            if(Math3D.IsEqual(edge, 2f)) return iCS_EdgeEnum.Right;
            if(Math3D.IsEqual(edge, 3f)) return iCS_EdgeEnum.Top;
            if(Math3D.IsEqual(edge, 4f)) return iCS_EdgeEnum.Bottom;
            return iCS_EdgeEnum.None;
        }
        set {
            switch(value) {
                case iCS_EdgeEnum.Left  : LocalAnchorPosition.x= 1f; break;
                case iCS_EdgeEnum.Right : LocalAnchorPosition.x= 2f; break;
                case iCS_EdgeEnum.Top   : LocalAnchorPosition.x= 3f; break;
                case iCS_EdgeEnum.Bottom: LocalAnchorPosition.x= 4f; break;
                default                 : LocalAnchorPosition.x= 0f; break;
            }
        }
    }
    public float PortPositionRatio {
        get { return LocalAnchorPosition.y; }
        set { LocalAnchorPosition.y= value; }
    }

    // ======================================================================
    // Initialization
    // ----------------------------------------------------------------------
    public iCS_EngineObject(int id, string name, Type type, int parentId, iCS_ObjectTypeEnum objectType) {
        Reset();
        ObjectType= objectType;
        InstanceId= id;
        ParentId= parentId;
        Name= name;
        QualifiedType= type.AssemblyQualifiedName;
        LocalAnchorPosition= Vector2.zero;
        if(IsDataPort) {
            Edge= IsInputPort ? (IsEnablePort ? iCS_EdgeEnum.Top : iCS_EdgeEnum.Left) : iCS_EdgeEnum.Right;
        }
    }
    // ----------------------------------------------------------------------
	public iCS_EngineObject() {
		Reset();
	}
    // ----------------------------------------------------------------------
	public static iCS_EngineObject CreateInvalidInstance() {
		return new iCS_EngineObject();
	}
    // ----------------------------------------------------------------------
	public void DestroyInstance() {
		Reset();
	}
    // ----------------------------------------------------------------------
    public static iCS_EngineObject Clone(int id, iCS_EngineObject toClone, iCS_EngineObject parent) {
        iCS_EngineObject instance= new iCS_EngineObject(id, toClone.Name, toClone.RuntimeType, parent != null ? parent.InstanceId : -1, toClone.ObjectType);
		// Commmon
        instance.DisplayOption= toClone.DisplayOption;
        instance.IsNameEditable= toClone.IsNameEditable;
		instance.LocalAnchorPosition= toClone.LocalAnchorPosition;
		// Node
		instance.MethodName= toClone.MethodName;
		instance.NbOfParams= toClone.NbOfParams;
        instance.IconGUID= toClone.IconGUID;
        instance.Tooltip= toClone.Tooltip;
		// Port
        instance.Edge= toClone.Edge;
        instance.PortIndex= toClone.PortIndex;
        if(instance.IsInDataPort && toClone.SourceId == -1 && !iCS_Types.IsA<UnityEngine.Object>(toClone.RuntimeType)) {
            instance.InitialValueArchive= toClone.InitialValueArchive;
        }
        return instance;
    }
    // ----------------------------------------------------------------------
    public void Reset() {
		// Common
        ObjectType= iCS_ObjectTypeEnum.Unknown;
        InstanceId= -1;
        ParentId= -1;
        QualifiedType= "";
		RawName= "";
        LocalAnchorPosition= Vector2.zero;
		LayoutPriority= 0;
		Scale= 1.0f;
        DisplayOption= iCS_DisplayOptionEnum.Unfolded;
        IsNameEditable= true;
		// Node
		MethodName= null;
		NbOfParams= 0;
        IconGUID= null;
        Tooltip = null;
		// Port
        Edge= iCS_EdgeEnum.None;
        SourceId= -1;
		PortIndex= -1;
		InitialValueArchive= null;
		// State
		IsEntryState= false;
    }
    
    // ----------------------------------------------------------------------
    // Object Type Acessor
    public bool IsNode                  { get { return iCS_ObjectType.IsNode(this); }}
	public bool IsStateChartNode	    { get { return iCS_ObjectType.IsStateChartNode(this); }}
    public bool IsBehaviour             { get { return iCS_ObjectType.IsBehaviour(this); }}
    public bool IsStateChart            { get { return iCS_ObjectType.IsStateChart(this); }}
    public bool IsState                 { get { return iCS_ObjectType.IsState(this); }}
    public bool IsModule                { get { return iCS_ObjectType.IsModule(this); }}
    public bool IsTransitionModule      { get { return iCS_ObjectType.IsTransitionModule(this); }}
    public bool IsTransitionGuard       { get { return iCS_ObjectType.IsTransitionGuard(this); }}
    public bool IsTransitionAction      { get { return iCS_ObjectType.IsTransitionAction(this); }}
    public bool IsFunction              { get { return iCS_ObjectType.IsFunction(this); }}
    public bool IsConstructor           { get { return iCS_ObjectType.IsConstructor(this); }}
    public bool IsClassMethod           { get { return iCS_ObjectType.IsClassMethod(this); }}
    public bool IsInstanceMethod        { get { return iCS_ObjectType.IsInstanceMethod(this); }}
    public bool IsClassField            { get { return iCS_ObjectType.IsClassField(this); }}
    public bool IsInstanceField         { get { return iCS_ObjectType.IsInstanceField(this); }}
    public bool IsTypeCast              { get { return iCS_ObjectType.IsTypeCast(this); }}
    public bool IsMessage               { get { return iCS_ObjectType.IsMessage(this); }}
    public bool IsPort                  { get { return iCS_ObjectType.IsPort(this); }}
    public bool IsDataPort              { get { return iCS_ObjectType.IsDataPort(this); }}
    public bool IsFunctionPort          { get { return iCS_ObjectType.IsFunctionPort(this); }}
    public bool IsModulePort            { get { return iCS_ObjectType.IsModulePort(this); }}
    public bool IsDynamicModulePort     { get { return iCS_ObjectType.IsDynamicModulePort(this); }}
    public bool IsStaticModulePort      { get { return iCS_ObjectType.IsStaticModulePort(this); }}
    public bool IsStatePort             { get { return iCS_ObjectType.IsStatePort(this); }}
    public bool IsTransitionPort        { get { return iCS_ObjectType.IsTransitionPort(this); }}
    public bool IsEnablePort            { get { return iCS_ObjectType.IsEnablePort(this); }}
    public bool IsOutputPort            { get { return iCS_ObjectType.IsOutputPort(this); }}
    public bool IsInputPort             { get { return iCS_ObjectType.IsInputPort(this); }}
    public bool IsInFixPort        { get { return iCS_ObjectType.IsInFixPort(this); }}
    public bool IsOutFixPort       { get { return iCS_ObjectType.IsOutFixPort(this); }}
    public bool IsInModulePort          { get { return iCS_ObjectType.IsInModulePort(this); }}
    public bool IsOutModulePort         { get { return iCS_ObjectType.IsOutModulePort(this); }}
    public bool IsInDynamicPort   { get { return iCS_ObjectType.IsInDynamicPort(this); }}
    public bool IsOutDynamicPort  { get { return iCS_ObjectType.IsOutDynamicPort(this); }}
    public bool IsInStaticModulePort    { get { return iCS_ObjectType.IsInStaticModulePort(this); }}
    public bool IsOutStaticModulePort   { get { return iCS_ObjectType.IsOutStaticModulePort(this); }}
    public bool IsInStatePort           { get { return iCS_ObjectType.IsInStatePort(this); }}
    public bool IsOutStatePort          { get { return iCS_ObjectType.IsOutStatePort(this); }}
    public bool IsInDataPort            { get { return iCS_ObjectType.IsInDataPort(this); }}
    public bool IsOutDataPort           { get { return iCS_ObjectType.IsOutDataPort(this); }}
    public bool IsInTransitionPort      { get { return iCS_ObjectType.IsInTransitionPort(this); }}
    public bool IsOutTransitionPort     { get { return iCS_ObjectType.IsOutTransitionPort(this); }}
    public bool IsObjectInstance        { get { return IsModule && RuntimeType != typeof(iCS_Module); }}
	public bool	IsMuxPort				{ get { return iCS_ObjectType.IsMuxPort(this); }}
	public bool IsChildMuxPort			{ get { return iCS_ObjectType.IsChildMuxPort(this); }}
	public bool IsParentMuxPort			{ get { return iCS_ObjectType.IsParentMuxPort(this); }}
    
    // ======================================================================
    // Feature support
    // ----------------------------------------------------------------------
    public bool SupportsAdditionOfPorts { get { return IsModule; }}
    public bool SupportsNestedNodes     { get { return IsModule; }}
    
    // ----------------------------------------------------------------------
	public FieldInfo GetFieldInfo() {
        if(MethodName == null) return null;
		Type classType= RuntimeType;
        FieldInfo field= classType.GetField(MethodName);
        if(field == null) {
            Debug.LogWarning("iCanScript: Unable to extract FieldInfo from RuntimeDesc: "+MethodName);                
        }
        return field;		            
	}

    // ----------------------------------------------------------------------
	public MethodBase GetMethodBase(List<iCS_EngineObject> engineObjects) {
        // Extract MethodBase for constructor.
        MethodBase method= null;
		Type classType= RuntimeType;
        if(ObjectType == iCS_ObjectTypeEnum.Constructor) {
            method= classType.GetConstructor(GetParamTypes(engineObjects));
            if(method == null) {
                string signature="(";
                bool first= true;
                foreach(var param in GetParamTypes(engineObjects)) {
                    if(first) { first= false; } else { signature+=", "; }
                    signature+= param.Name;
                }
                signature+=")";
                Debug.LogWarning("Unable to extract constructor: "+classType.Name+signature);
            }
            return method;
        }
        // Extract MethodBase for class methods.
        if(MethodName == null) return null;
		Type[] paramTypes= GetParamTypes(engineObjects);
        method= classType.GetMethod(MethodName, paramTypes);            
        if(method == null) {
            string signature="(";
            bool first= true;
            foreach(var param in paramTypes) {
                if(first) { first= false; } else { signature+=", "; }
                signature+= param.Name;
            }
            signature+=")";
            Debug.LogWarning("iCanScript: Unable to extract MethodInfo from RuntimeDesc: "+MethodName+signature);
        }
        return method;		            
	}
	public Type[] GetParamTypes(List<iCS_EngineObject> engineObjects) {
		iCS_EngineObject[] ports= GetChildPorts(engineObjects);
		Type[] result= new Type[NbOfParams];
		for(int i= 0; i < result.Length; ++i) {
			result[i]= ports[i].RuntimeType;
		}
		return result;	        
	}
	iCS_EngineObject[] GetChildPorts(List<iCS_EngineObject> engineObjects) {
		List<iCS_EngineObject> ports= new List<iCS_EngineObject>();
		// Get all child data ports.
		int nodeId= InstanceId;
		foreach(var port in engineObjects) {
			if(port == null) continue;
			if(port.ParentId != nodeId) continue;
			if(!port.IsDataPort) continue;
			if(port.IsEnablePort) continue;
			ports.Add(port);
		}
		// Sort child ports according to index.
		iCS_EngineObject[] result= ports.ToArray();
		Array.Sort(result, (x,y)=> x.PortIndex - y.PortIndex);
		return result;            
	}

}
