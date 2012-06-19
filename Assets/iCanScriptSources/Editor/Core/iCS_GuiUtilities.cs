using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public static class iCS_GuiUtilities {
    class GUIFieldInfo {
        public bool    Foldout= false;
        public object  Value= null;
		public int	   ControlID= -1;
        public GUIFieldInfo(bool foldout) { Foldout= foldout; }
        public GUIFieldInfo(object value) { Value= value; }
    }
    
    // -----------------------------------------------------------------------
    static void AddFoldout(Dictionary<string,object> db, string key, bool foldout) { db.Add(key, new GUIFieldInfo(foldout)); }
    static bool Foldout(Dictionary<string,object> db, string key)                  { return ((GUIFieldInfo)(db[key])).Foldout; }
    static void Foldout(Dictionary<string,object> db, string key, bool foldout)    { ((GUIFieldInfo)(db[key])).Foldout= foldout; }
    static void   AddValue(Dictionary<string,object> db, string key, object value) { db.Add(key, new GUIFieldInfo(value)); }
    static object Value(Dictionary<string,object> db, string key)                  { return ((GUIFieldInfo)(db[key])).Value; }
    static void   Value(Dictionary<string,object> db, string key, object value)    { ((GUIFieldInfo)(db[key])).Value= value; }
    static int  ControlID(Dictionary<string,object> db, string key)                { return ((GUIFieldInfo)(db[key])).ControlID; }
    static void ControlID(Dictionary<string,object> db, string key, int value)     { ((GUIFieldInfo)(db[key])).ControlID= value; }

    // -----------------------------------------------------------------------
    public static void OnInspectorDataPortGUI(iCS_EditorObject port, iCS_IStorage storage, int indentLevel, Dictionary<string,object> foldoutDB) {
        // Only accept data ports.
        if(!port.IsDataPort) return;
        // Extract port information
		Type portType= port.RuntimeType;
        iCS_EditorObject parent= storage.GetParent(port);
        iCS_EditorObject sourcePort= storage.GetSource(port);
        bool hasSource= sourcePort != null;
        // Get runtime object if it exists.
        iCS_IParams runtimeObject= storage.GetRuntimeObject(parent) as iCS_IParams;
        // Determine if we are allowed to modify port value.
        bool isReadOnly= !(!hasSource && (port.IsInputPort || port.IsModulePort));
        // Nothing to display if we don't have a runtime object and we are in readonly.
        if(isReadOnly && runtimeObject == null) return;
        // Update port value from runtime object in priority or the descriptor string if no runtime.
		object portValue= storage.GetPortValue(port);
        // Determine section name (used for foldout parent).
        string foldoutName= (port.IsInputPort ? "in" : "out")+"."+parent.Name;
        // Display primitives.
        bool isDirty= false;
        object newPortValue= ShowInInspector(port.Name, isReadOnly, hasSource, foldoutName, portType, portValue, indentLevel, foldoutDB, ref isDirty);
        if(!isReadOnly && isDirty) {
			storage.UpdatePortInitialValue(port, newPortValue);
        }
    }

    // -----------------------------------------------------------------------
    public static object ShowInInspector(string name, bool isReadOnly, bool hasSource, string compositeParent,
                                         Type baseType, object currentValue,
                                         int indentLevel, Dictionary<string,object> foldoutDB, ref bool isDirty) {
		// Extract type information.
        Type valueType= currentValue != null ? currentValue.GetType() : baseType;
        Type baseElementType= iCS_Types.GetElementType(baseType);
        Type valueElementType= iCS_Types.GetElementType(valueType);
        EditorGUI.indentLevel= indentLevel;
		// Make nice name for field to edit.
        string niceName= name == null || name == "" ? "(Unamed)" : ObjectNames.NicifyVariableName(name);
        if(baseType.IsArray) niceName= "["+niceName+"]";
        // Special case for readonly & null value.
        if(isReadOnly && currentValue == null) {
            EditorGUILayout.LabelField(niceName, hasSource ? "(see connection)":"(not available)");
            return currentValue;
        }
        // Special case for arrays
		if(baseType.IsArray) {
			if(currentValue == null) {
				currentValue= Array.CreateInstance(baseElementType, 0);
				isDirty= true;
				return currentValue;
			}			
            string compositeArrayName= compositeParent+"."+name;
            if(!foldoutDB.ContainsKey(compositeArrayName)) AddFoldout(foldoutDB, compositeArrayName, false);
            bool showArray= Foldout(foldoutDB, compositeArrayName);
            showArray= EditorGUILayout.Foldout(showArray, niceName);
            Foldout(foldoutDB, compositeArrayName, showArray);
			if(!showArray) return currentValue;
            EditorGUI.indentLevel= indentLevel+1;
            Array array= currentValue as Array;
            int newSize= array.Length;
            if(ModalEdit("Length", "Length", ref newSize, compositeArrayName, (n,v)=> EditorGUILayout.IntField(n,v), foldoutDB)) {
                if(newSize != array.Length) {
					if(newSize < 100 || EditorUtility.DisplayDialog("Resizing array", "The new size of the array is > 100.  Are you sure you want your new array to be resized to "+newSize+".", "Resize", "Cancel")) {
	                    Array newArray= Array.CreateInstance(baseElementType, newSize);
	                    Array.Copy(array, newArray, Mathf.Min(newSize, array.Length));
	                    array= newArray;
	                    isDirty= true;							
						return array;
					}
                }					
			} 
            for(int i= 0; i < array.Length; ++i) {
				bool elemDirty= false;
                object newValue= ShowInInspector("["+i+"]", isReadOnly, hasSource, compositeArrayName, baseElementType, array.GetValue(i), indentLevel+1, foldoutDB, ref elemDirty);
				isDirty |= elemDirty;
				if(elemDirty) array.SetValue(newValue, i);
            }
            return array;
		}
        // Support all UnityEngine objects.
        if(iCS_Types.IsA<UnityEngine.Object>(baseElementType)) {
            UnityEngine.Object value= currentValue != null ? currentValue as UnityEngine.Object: null;
            UnityEngine.Object newValue= EditorGUILayout.ObjectField(niceName, value, baseElementType, true);
			if(value == null && newValue == null) return newValue;
			if(value != newValue ) isDirty= true;
			return newValue;
        }        
        // Support Type type.
        if(valueElementType == typeof(Type) || currentValue is Type) {
            string typeName= currentValue != null ? (currentValue as Type).FullName : "";
			string origTypeName= typeName;
            if(ModalEdit(niceName, name, ref typeName, compositeParent, (n,v)=> EditorGUILayout.TextField(n,v), foldoutDB)) {
                Type newType= Type.GetType(typeName);
                if(newType != null) {
                    isDirty= true;
                    return newType;
                }
                else {
                    Value(foldoutDB, compositeParent+"."+name, origTypeName);
                    Debug.LogWarning("Type: "+typeName+" was not found.");
//					EditorWindow.GetWindow(typeof(iCS_Editor), false, "iCanScript").ShowNotification(new GUIContent("Type: '"+typeName+"' cannot be found.  Are you missing a namespace?"));
                }
            } 
            return currentValue;
        }
        // Determine if we should create a value if the current value is null.
        if(currentValue == null) {
            // Automatically create value types.
            if(baseElementType.IsValueType || baseElementType.IsEnum) {
                currentValue= iCS_Types.CreateInstance(baseElementType);                    
                isDirty= true;
				return currentValue;
            } else { // Ask to create reference types.
                Type[] derivedTypes= iCS_Reflection.GetAllTypesWithDefaultConstructorThatDeriveFrom(baseElementType);
                if(derivedTypes.Length <= 1) {
					isDirty= true;
                    return iCS_Types.CreateInstance(baseType);
                }
                string[] typeNames= new string[derivedTypes.Length+1];
                typeNames[0]= "None";
                if(baseType.IsArray) {
                    for(int i= 0; i < derivedTypes.Length; ++i) typeNames[i+1]= iCS_Types.GetName(derivedTypes[i])+"[]";                                        
                } else {
                    for(int i= 0; i < derivedTypes.Length; ++i) typeNames[i+1]= iCS_Types.GetName(derivedTypes[i]);                    
                }
                int idx= EditorGUILayout.Popup(niceName, 0, typeNames);
                if(idx == 0) return null;
                isDirty= true;
                if(baseType.IsArray) {
                    return Array.CreateInstance(derivedTypes[idx-1],0);
                }
                return iCS_Types.CreateInstance(derivedTypes[idx-1]);
            }
        }
        // Special case for enumerations
        if(valueElementType.IsEnum) {
            if(currentValue == null) { return currentValue; }
            System.Enum value= currentValue as System.Enum;
            System.Enum newValue= EditorGUILayout.EnumPopup(niceName, value);
            if(newValue.ToString().CompareTo(value.ToString()) != 0) isDirty= true;
            return newValue;
        }
        // C# data types.
        if(valueElementType == typeof(byte)) {
            byte value= (byte)currentValue;
            byte newValue= (byte)((int)EditorGUILayout.IntField(niceName, (int)value));
            if(newValue != value) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(sbyte)) {
            sbyte value= (sbyte)currentValue;
            sbyte newValue= (sbyte)((int)EditorGUILayout.IntField(niceName, (int)value));            
            if(newValue != value) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(bool)) {
            bool value= (bool)currentValue;
            bool newValue= EditorGUILayout.Toggle(niceName, value);
            if(newValue != value) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(int)) {
            int value= (int)currentValue;
            int newValue= EditorGUILayout.IntField(niceName, value);
            if(newValue != value) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(uint)) {
            string uintAsString= (string)Convert.ChangeType((uint)currentValue, typeof(string));
            string newUIntAsString= EditorGUILayout.TextField(niceName, uintAsString);
            if(uintAsString.CompareTo(newUIntAsString) != 0) isDirty= true;
            return Convert.ChangeType(newUIntAsString, typeof(uint));
        }
        if(valueElementType == typeof(short)) {
            short value= (short)currentValue;
            short newValue= (short)((int)EditorGUILayout.IntField(niceName, (int)value));            
            if(newValue != value) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(ushort)) {
            int value= (ushort)currentValue;
            int newValue= (ushort)((int)EditorGUILayout.IntField(niceName, (int)value));            
            if(newValue != value) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(long)) {
            string longAsString= (string)Convert.ChangeType((long)currentValue, typeof(string));
            string newLongAsString= EditorGUILayout.TextField(niceName, longAsString);
            if(longAsString.CompareTo(newLongAsString) != 0) isDirty= true;
            return Convert.ChangeType(newLongAsString, typeof(long));
        }
        if(valueElementType == typeof(ulong)) {
            string ulongAsString= (string)Convert.ChangeType((ulong)currentValue, typeof(string));
            string newULongAsString= EditorGUILayout.TextField(niceName, ulongAsString);
            if(ulongAsString.CompareTo(newULongAsString) != 0) isDirty= true;
            return Convert.ChangeType(newULongAsString, typeof(ulong));
        }
        if(valueElementType == typeof(float)) {
			float value= (float)currentValue;
            float newValue= EditorGUILayout.FloatField(niceName, (float)currentValue);
			if(newValue != value) isDirty= true;
			return newValue;
        }
        if(valueElementType == typeof(double)) {
            string doubleAsString= (string)Convert.ChangeType((double)currentValue, typeof(string));
            string newDoubleAsString= EditorGUILayout.TextField(niceName, doubleAsString);
            if(doubleAsString.CompareTo(newDoubleAsString) != 0) isDirty= true;
            return Convert.ChangeType(newDoubleAsString, typeof(double));
        }
        if(valueElementType == typeof(decimal)) {
            float valueAsFloat= (float)((decimal)currentValue);
            float newDecimalAsFloat= EditorGUILayout.FloatField(niceName, valueAsFloat);
            if(valueAsFloat != newDecimalAsFloat) isDirty= true;
            return (decimal)newDecimalAsFloat;
        }
        if(valueElementType == typeof(char)) {
            string value= ""+((char)currentValue);
            string newCharAsString= EditorGUILayout.TextField(niceName, value);
			if(newCharAsString == null || newCharAsString == "" || newCharAsString[0] == 0) newCharAsString= " ";
            if(newCharAsString[0] != value[0]) isDirty= true;
            return (newCharAsString != null && newCharAsString.Length >= 1) ? newCharAsString[0] : default(char);
        }
        if(valueElementType == typeof(string)) {
            string value= ((string)currentValue) ?? "";
            string newValue= EditorGUILayout.TextField(niceName, value);
            if(newValue != value) isDirty= true;
            return newValue;
        }
        // Unity data types.
        if(valueElementType == typeof(Vector2)) {
            Vector2 value= (Vector2)currentValue;
            Vector2 newValue= EditorGUILayout.Vector2Field(niceName, value);
            if(Math3D.IsNotEqual(newValue, value)) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(Vector3)) {
            Vector3 value= (Vector3)currentValue;
            Vector3 newValue= EditorGUILayout.Vector3Field(niceName, value);
            if(Math3D.IsNotEqual(newValue, value)) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(Vector4)) {
            Vector4 value= (Vector4)currentValue;
            Vector4 newValue= EditorGUILayout.Vector4Field(niceName, value);
            if(Math3D.IsNotEqual(newValue, value)) isDirty= true;
            return newValue;
        }
        if(valueElementType == typeof(Color)) {
            Color value= (Vector4)currentValue;
            Color newValue= EditorGUILayout.ColorField(niceName, value);
            if(Math3D.IsNotEqual(newValue.r, value.r) ||
               Math3D.IsNotEqual(newValue.g, value.g) ||
               Math3D.IsNotEqual(newValue.b, value.b) ||
               Math3D.IsNotEqual(newValue.a, value.a)) {
                isDirty= true;
            }
            return newValue;
        }
		// All other types.
        string compositeName= compositeParent+"."+name;
        if(!foldoutDB.ContainsKey(compositeName)) AddFoldout(foldoutDB, compositeName, false);
        bool showCompositeObject= Foldout(foldoutDB, compositeName);
        showCompositeObject= EditorGUILayout.Foldout(showCompositeObject, niceName);
        Foldout(foldoutDB, compositeName, showCompositeObject);
        if(showCompositeObject) {
    		foreach(var field in valueElementType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                bool shouldInspect= true;
                if(field.IsPublic) {
                    foreach(var attribute in field.GetCustomAttributes(true)) {
                        if(attribute is System.NonSerializedAttribute) { shouldInspect= false; break; }
                        if(attribute is HideInInspector) { shouldInspect= false; break; }
                    }
                } else {
                    shouldInspect= false;
                    foreach(var attribute in field.GetCustomAttributes(true)) {
                        if(attribute is SerializeField) shouldInspect= true;
                        if(attribute is HideInInspector) { shouldInspect= false; break; }
                    }                
                }
                if(shouldInspect) {
                    object currentFieldValue= field.GetValue(currentValue);
                    bool isFieldDirty= false;
                    object newFieldValue= ShowInInspector(field.Name, isReadOnly, hasSource, compositeName, field.FieldType, currentFieldValue, indentLevel+1, foldoutDB, ref isFieldDirty);
                    isDirty |= isFieldDirty;
                    if(!isReadOnly && isFieldDirty) {
                        field.SetValue(currentValue, newFieldValue);
                    }
                }
    		}        
        }
        return currentValue;
    }

    // ----------------------------------------------------------------------
    static bool ModalEdit<T>(string niceName, string name, ref T currentValue, string parentName, Func<string,T,T> editor, Dictionary<string,object> db) {
        string controlName= parentName+"."+name;
		if(!db.ContainsKey(controlName)) AddValue(db, controlName, currentValue);
        T value= (T)Value(db, controlName);
		GUI.SetNextControlName(controlName);
        T newValue= editor(niceName, value);
        Value(db, controlName, newValue);
		int keyControlID= GUIUtility.keyboardControl;
		if(GUI.GetNameOfFocusedControl() == controlName) ControlID(db, controlName, keyControlID);
		int savedKeyControlID= ControlID(db, controlName);
		if(savedKeyControlID == -1) return false;
        if(savedKeyControlID != keyControlID) {
			ControlID(db, controlName, -1);
		    currentValue= newValue;
		    return true;
	    }
        return false;
    }
    
    // -----------------------------------------------------------------------
    public static void UnsupportedFeature() {
        Debug.LogWarning("The selected feature is unsupported in the current version of iCanScript.  Feature is planned for a later version.  Thanks for your patience.");
    }
}
