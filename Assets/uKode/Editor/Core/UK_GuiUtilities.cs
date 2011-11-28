using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class UK_GuiUtilities {
    public static void OnInspectorGUI(UK_EditorObject port, UK_IStorage storage) {
        // Extract port information
        string niceName= port.Name == null || port.Name == "" ? "(Unamed)" : ObjectNames.NicifyVariableName(port.Name);
        Type dataType= UK_Types.GetDataType(port.RuntimeType);
        UK_EditorObject node= storage.GetParent(port);
        int portId= port.PortIndex;
        // Extract parent node information.
        UK_RuntimeDesc desc= new UK_RuntimeDesc(node.RuntimeArchive);    
        // Get runtime object if it exists.
        UK_FunctionBase runtimeObject= storage.GetRuntimeObject(node) as UK_FunctionBase;
        // Update port value from runtime object in priority or the descriptor string if no runtime.
        object portValue= null;
        if(runtimeObject != null) {
            portValue= runtimeObject[portId];
        } else if(portId < desc.ParamIsOuts.Length && !desc.ParamIsOuts[portId]) {
            portValue= storage.GetDefaultValue(desc, portId);            
        } else {
            portValue= UK_Types.DefaultValue(dataType);
        }

        // Display primitives.
        if(dataType == typeof(bool)) {
            bool value= portValue != null ? (bool)portValue : default(bool);
            bool newValue= EditorGUILayout.Toggle(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;
        }
        if(dataType == typeof(int)) {
            int value= portValue != null ? (int)portValue : default(int);
            int newValue= EditorGUILayout.IntField(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;
        }
        if(dataType == typeof(float)) {
            float value= portValue != null ? (float)portValue : default(float);
            float newValue= EditorGUILayout.FloatField(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;
        }
        if(dataType == typeof(string)) {
            string value= ((string)portValue) ?? "";
            string newValue= EditorGUILayout.TextField(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;
        }
        if(dataType == typeof(Vector2)) {
            Vector2 value= portValue != null ? (Vector2)portValue : default(Vector2);
            Vector2 newValue= EditorGUILayout.Vector2Field(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;            
        }
        if(dataType == typeof(Vector3)) {
            Vector3 value= portValue != null ? (Vector3)portValue : default(Vector3);
            Vector3 newValue= EditorGUILayout.Vector3Field(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;            
        }
        if(dataType == typeof(Vector4)) {
            Vector4 value= portValue != null ? (Vector4)portValue : default(Vector4);
            Vector4 newValue= EditorGUILayout.Vector4Field(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;            
        }
        if(dataType == typeof(Color)) {
            Color value= portValue != null ? (Color)portValue : default(Color);
            Color newValue= EditorGUILayout.ColorField(niceName, value);
            if(port.IsInputPort && runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;            
        }
        // Suport all UnityEngine objects.
        if(UK_Types.IsA<UnityEngine.Object>(dataType)) {
            UnityEngine.Object value= portValue != null ? portValue as UnityEngine.Object: null;
            UnityEngine.Object newValue= EditorGUILayout.ObjectField(niceName, value, dataType, true);
            if(port.IsInputPort & runtimeObject != null) runtimeObject[portId]= newValue;
            if(value != newValue && storage.GetSource(port) == null) {
                storage.SetDefaultValue(desc, portId, newValue);
                node.RuntimeArchive= desc.Encode(desc.Id);
            }
            return;                                    
        }

//        // Display array of primitives.
//        if(fieldType.IsArray) {
//            System.Type elementType= fieldType.GetElementType();
//            if(elementType == typeof(bool)) {
//                bool[] values= (bool[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.Toggle(niceName+"["+i+"]", values[i]);
//                }
//                return;
//            }
//            if(elementType == typeof(int)) {
//                int[] values= (int[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.IntField(niceName+"["+i+"]", values[i]);
//                }
//                return;
//            }
//            if(elementType == typeof(float)) {
//                float[] values= (float[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.FloatField(niceName+"["+i+"]", values[i]);
//                }
//                return;            
//            }
//            if(elementType == typeof(string)) {
//                string[] values= (string[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.TextField(niceName+"["+i+"]", values[i]);
//                }
//                return;
//            }
//            if(elementType == typeof(Vector2)) {
//                Vector2[] values= (Vector2[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.Vector2Field(niceName+"["+i+"]", values[i]);
//                }
//                return;                        
//            }            
//            if(elementType == typeof(Vector3)) {
//                Vector3[] values= (Vector3[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.Vector3Field(niceName+"["+i+"]", values[i]);
//                }
//                return;                        
//            }            
//            if(elementType == typeof(Vector4)) {
//                Vector4[] values= (Vector4[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.Vector4Field(niceName+"["+i+"]", values[i]);
//                }
//                return;                        
//            }            
//            if(elementType == typeof(GameObject)) {
//                GameObject[] values= (GameObject[])obj;
//                for(int i= 0; i < values.Length; ++i) {
//                    values[i]= EditorGUILayout.ObjectField(niceName+"["+i+"]", values[i], typeof(GameObject), true) as GameObject;
//                }
//                return;                        
//            }                        
//            Debug.LogWarning("OnInspectorGUI: Unsupport array type: "+fieldInfo.FieldType.Name);
//            return;
//        }
//
//        // Display list of primitives.
//        if(obj is List<bool>) {
//            List<bool> values= (List<bool>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.Toggle(niceName+"["+i+"]", values[i]);
//            }
//            return;
//        }
//        if(obj is List<int>) {
//            List<int> values= (List<int>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.IntField(niceName+"["+i+"]", values[i]);
//            }
//            return;
//        }
//        if(obj is List<float>) {
//            List<float> values= (List<float>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.FloatField(niceName+"["+i+"]", values[i]);
//            }
//            return;            
//        }
//        if(obj is List<string>) {
//            List<string> values= (List<string>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.TextField(niceName+"["+i+"]", values[i]);
//            }
//            return;
//        }
//        if(obj is List<Vector2>) {
//            List<Vector2> values= (List<Vector2>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.Vector2Field(niceName+"["+i+"]", values[i]);
//            }
//            return;                        
//        }            
//        if(obj is List<Vector3>) {
//            List<Vector3> values= (List<Vector3>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.Vector3Field(niceName+"["+i+"]", values[i]);
//            }
//            return;                        
//        }            
//        if(obj is List<Vector4>) {
//            List<Vector4> values= (List<Vector4>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.Vector4Field(niceName+"["+i+"]", values[i]);
//            }
//            return;                        
//        }            
//        if(obj is List<GameObject>) {
//            List<GameObject> values= (List<GameObject>)obj;
//            for(int i= 0; i < values.Count; ++i) {
//                values[i]= EditorGUILayout.ObjectField(niceName+"["+i+"]", values[i], typeof(GameObject), true) as GameObject;
//            }
//            return;                        
//        }            
//
        Debug.LogWarning("OnInspectorGUI: Unknown type: "+port.RuntimeType);
    }
}
