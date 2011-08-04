using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class AP_ObjectUtil : ScriptableObject {
    // ======================================================================
    // OBJECT IDENTIFICATION UTILITIES
    // ----------------------------------------------------------------------
    public bool         IsRootNode  { get { return null != AsRootNode; }}
    public bool         IsTop       { get { return null != AsTop; }}
    public bool         IsNode      { get { return null != AsNode; }}
    public bool         IsPort      { get { return null != AsPort; }}
    public AP_RootNode  AsRootNode  { get { return this as AP_RootNode; }}
    public AP_Top       AsTop       { get { return this as AP_Top; }}
    public AP_Node      AsNode      { get { return this as AP_Node; }}
    public AP_Port      AsPort      { get { return this as AP_Port; }}
    

    // ======================================================================
    // PUBLISHING UTILITIES
    // ----------------------------------------------------------------------
    // Returns the list of defined input fields.
    public List<FieldInfo> GetInputFields() {
        List<FieldInfo> list= new List<FieldInfo>();
        System.Type objType= GetType();
        foreach(var field in objType.GetFields()) {
            foreach(var attribute in field.GetCustomAttributes(true)) {
                if((attribute is AP_InPortAttribute) || (attribute is AP_InOutPortAttribute)) {
                    list.Add(field);
                }
            }
        }        
        return list;
    }
    // ----------------------------------------------------------------------
    // Returns the list of defined output fields.
    public List<FieldInfo> GetOutputFields() {
        List<FieldInfo> list= new List<FieldInfo>();
        System.Type objType= GetType();
        foreach(var field in objType.GetFields()) {
            foreach(var attribute in field.GetCustomAttributes(true)) {
                if((attribute is AP_OutPortAttribute) || (attribute is AP_InOutPortAttribute)) {
                    list.Add(field);
                }
            }
        }        
        return list;
    }
    // ----------------------------------------------------------------------
    // Returns the field info of the named input field.
    public FieldInfo GetInputField(string name) {
        System.Type objType= GetType();
        foreach(var field in objType.GetFields()) {
            foreach(var attribute in field.GetCustomAttributes(true)) {
                if((attribute is AP_InPortAttribute) || (attribute is AP_InOutPortAttribute)) {
                    if(field.Name == name) {
                        return field;
                    }
                }
            }
        }        
        return null;
    }
    // ----------------------------------------------------------------------
    // Returns the field info of the named output field.
    public FieldInfo GetOutputField(string name) {
        System.Type objType= GetType();
        foreach(var field in objType.GetFields()) {
            foreach(var attribute in field.GetCustomAttributes(true)) {
                if((attribute is AP_OutPortAttribute) || (attribute is AP_InOutPortAttribute)) {
                    if(field.Name == name) {
                        return field;
                    }
                }
            }
        }        
        return null;
    }

    // ----------------------------------------------------------------------
    // Returns the list of attribute defined input properties.
    public List<PropertyInfo> GetInputProperties() {
        List<PropertyInfo> list= new List<PropertyInfo>();
        System.Type objType= GetType();
        foreach(var property in objType.GetProperties()) {
            foreach(var attribute in property.GetCustomAttributes(true)) {
                if((attribute is AP_InPropertyAttribute) || (attribute is AP_InOutPropertyAttribute)) {
                    list.Add(property);
                }
            }
        }        
        return list;
    }
    // ----------------------------------------------------------------------
    // Returns the list of attribute defined output properties.
    public List<PropertyInfo> GetOutputProperties() {
        List<PropertyInfo> list= new List<PropertyInfo>();
        System.Type objType= GetType();
        foreach(var property in objType.GetProperties()) {
            foreach(var attribute in property.GetCustomAttributes(true)) {
                if((attribute is AP_OutPropertyAttribute) || (attribute is AP_InOutPropertyAttribute)) {
                    list.Add(property);
                }
            }
        }        
        return list;
    }
    // ----------------------------------------------------------------------
    // Returns the property info of the named input properties.
    public PropertyInfo GetInputProperty(string name) {
        System.Type objType= GetType();
        foreach(var property in objType.GetProperties()) {
            foreach(var attribute in property.GetCustomAttributes(true)) {
                if((attribute is AP_InPropertyAttribute) || (attribute is AP_InOutPropertyAttribute)) {
                    if(property.Name == name) {
                        return property;
                    }
                }
            }
        }        
        return null;
    }
    // ----------------------------------------------------------------------
    // Returns the property info of the named output properties.
    public PropertyInfo GetOutputProperty(string name) {
        System.Type objType= GetType();
        foreach(var property in objType.GetProperties()) {
            foreach(var attribute in property.GetCustomAttributes(true)) {
                if((attribute is AP_OutPropertyAttribute) || (attribute is AP_InOutPropertyAttribute)) {
                    if(property.Name == name) {
                        return property;
                    }
                }
            }
        }        
        return null;
    }
    
    // ======================================================================
    // FUNCTIONAL UTILITIES
    // ----------------------------------------------------------------------
    // Excutes the given action if the given object matches the T type.
    public void ExecuteIf<T>(System.Action<T> fnc) where T : AP_Object {
        Prelude.executeIf<T>(this, fnc);
    }

    // ----------------------------------------------------------------------
    // Excutes the given function if the given object matches the T type.
    public R ExecuteIf<T,R>(System.Func<T,R> fnc, R defaultReturn) where T : AP_Object {
        return Prelude.executeIf<T,R>(this, fnc, defaultReturn);
    }

    // ----------------------------------------------------------------------
    // Prints unknnown type on debug console.
    public static void PrintUnknown(System.Object obj) {
        Debug.Log(obj.GetType().FullName + " is an unsuported type for this case statement.");
    }
    
    // ----------------------------------------------------------------------
    // Executes a case statement according to the type of the object
    public void Case<T1,T2>(System.Action<T1> f1,
                            System.Action<T2> f2,
                            System.Action<AP_Object> defaultFnc= null) where T1 : AP_Object
                                                                       where T2 : AP_Object {
        Prelude.choice<T1,T2>(this, f1, f2,
            (obj)=>{
                if(defaultFnc == null) PrintUnknown(obj);
                else                   defaultFnc(obj as AP_Object);
            }
        );
    }

    // ----------------------------------------------------------------------
    // Executes a case statement according to the type of the object
    public void Case<T1,T2,T3>(System.Action<T1> f1,
                               System.Action<T2> f2,
                               System.Action<T3> f3,
                               System.Action<AP_Object> defaultFnc= null) where T1 : AP_Object
                                                                          where T2 : AP_Object
                                                                          where T3 : AP_Object {
        Prelude.choice<T1,T2,T3>(this, f1, f2, f3,
            (obj)=>{
                if(defaultFnc == null) PrintUnknown(obj);
                else                   defaultFnc(obj as AP_Object);
            }
        );
    }
    
    // ----------------------------------------------------------------------
    // Executes a case statement according to the type of the object
    public void Case<T1,T2,T3,T4>(System.Action<T1> f1,
                                  System.Action<T2> f2,
                                  System.Action<T3> f3,
                                  System.Action<T4> f4,
                                  System.Action<AP_Object> defaultFnc= null) where T1 : AP_Object
                                                                             where T2 : AP_Object
                                                                             where T3 : AP_Object
                                                                             where T4 : AP_Object {
        Prelude.choice<T1,T2,T3,T4>(this, f1, f2, f3, f4,
            (obj)=>{
                if(defaultFnc == null) PrintUnknown(obj);
                else                   defaultFnc(obj as AP_Object);
            }
        );
    }
    
    // ----------------------------------------------------------------------
    // Executes a case statement according to the type of the object
    public void Case<T1,T2,T3,T4,T5>(System.Action<T1> f1,
                                     System.Action<T2> f2,
                                     System.Action<T3> f3,
                                     System.Action<T4> f4,
                                     System.Action<T5> f5,
                                     System.Action<AP_Object> defaultFnc= null) where T1 : AP_Object
                                                                                where T2 : AP_Object
                                                                                where T3 : AP_Object
                                                                                where T4 : AP_Object
                                                                                where T5 : AP_Object {
        Prelude.choice<T1,T2,T3,T4,T5>(this, f1, f2, f3, f4, f5,
            (obj)=>{
                if(defaultFnc == null) PrintUnknown(obj);
                else                   defaultFnc(obj as AP_Object);
            }
        );
    }
}
