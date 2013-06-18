using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public abstract class iCS_MemberInfo {
    // ======================================================================
    // Constants
    // ----------------------------------------------------------------------
    // TODO: Use periods (.) instead of slashes (/) for type string.
    public const string memberSeparator= "/";
    
    // ======================================================================
    // Fields
    // ----------------------------------------------------------------------
    public iCS_ObjectTypeEnum   objectType      = iCS_ObjectTypeEnum.Unknown;
    public iCS_TypeInfo         parentTypeInfo  = null;
    public string               displayName     = null;
    public string               description     = null;
    public string               iconPath        = null;

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_MemberInfo(iCS_ObjectTypeEnum _objType, iCS_TypeInfo _parentTypeInfo,
                          string _name, string _description, string _iconPath) {
		objectType    = _objType;
		parentTypeInfo= _parentTypeInfo;
		displayName   = _name;
        description   = _description;
        iconPath      = _iconPath;
		// Register ourself in parent type.
		if(_parentTypeInfo != null) {
			_parentTypeInfo.members.Add(this);			
		}

    }

    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public iCS_TypeInfo         toTypeInfo          { get { return this as iCS_TypeInfo; }}
    public iCS_ConstructorInfo  toConstructorInfo   { get { return this as iCS_ConstructorInfo; }}
    public iCS_MethodInfo       toMethodInfo        { get { return this as iCS_MethodInfo; }}
    public iCS_FieldInfo        toFieldInfo         { get { return this as iCS_FieldInfo; }}
    public iCS_PropertyInfo     toPropertyInfo      { get { return this as iCS_PropertyInfo; }}
    public iCS_EventInfo        toEventInfo         { get { return this as iCS_EventInfo; }}
	public iCS_TypeCastInfo		toTypeCastInfo		{ get { return this as iCS_TypeCastInfo; }}
    // ----------------------------------------------------------------------
    public bool isGlobalScope         { get { return parentTypeInfo == null; }}
    // ----------------------------------------------------------------------
    public bool isTypeInfo            { get { return toTypeInfo != null; }}
    public bool isConstructor         { get { return toConstructorInfo != null; }}
    public bool isMethod              { get { return toMethodInfo != null; }}
    public bool isField               { get { return toFieldInfo != null; }}
    public bool isEvent               { get { return toEventInfo != null; }}
    public bool isProperty            { get { return toPropertyInfo != null; }}
	public bool isTypeCast			  { get { return toTypeCastInfo != null; }}
    public bool isInstanceField       { get { return isField && toFieldInfo.isInstanceMember; }}
    public bool isStaticField         { get { return isField && toFieldInfo.isClassMember; }}
    public bool isGetField            { get { return isField && toFieldInfo.isGet; }}
    public bool isSetField            { get { return isField && toFieldInfo.isSet; }}     
    public bool isGetInstanceField    { get { return isInstanceField && isGetField; }}
    public bool isSetInstanceField    { get { return isInstanceField && isSetField; }}
    public bool isGetStaticField      { get { return isStaticField && isGetField; }}
    public bool isSetStaticField      { get { return isStaticField && isSetField; }}
    public bool isGetProperty         { get { return isProperty && toPropertyInfo.isGet; }}
    public bool isSetProperty         { get { return isProperty && toPropertyInfo.isSet; }}
    public bool isGetInstanceProperty { get { return isGetProperty && toPropertyInfo.isInstanceMember; }}
    public bool isSetInstanceProperty { get { return isSetProperty && toPropertyInfo.isInstanceMember; }}
    public bool isGetStaticProperty   { get { return isGetProperty && toPropertyInfo.isClassMember; }}
    public bool isSetStaticProperty   { get { return isSetProperty && toPropertyInfo.isClassMember; }}

    // ======================================================================
    // Dynamic Properties
    // ----------------------------------------------------------------------
    public string company {
        var _company= null;
        if(parentTypeInfo != null) {
            _company= parentTypeInfo.company;
        }
        return String.IsEmptyOrNull(_company) ? GetCompany() : _company;
    }
    public string package {
        var _package= GetPackage();
        return String.IsEmptyOrNull(_package) && parentTypeInfo != null ? parentTypeInfo.package : _package;
    }
    public string description {
        var _description= GetDescription();
        return String.IsEmptyOrNull(_description) && parentTypeInfo != null ? parentTypeInfo.description : _description;
    }
    public string iconPath {
        var _iconPath= GetIconPath();
        return String.IsEmptyOrNull(_iconPath) && parentTypeInfo != null ? parentTypeInfo.iconPath : _iconPath;
    }
}
