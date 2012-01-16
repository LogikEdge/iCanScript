using UnityEngine;
using System.Collections;

[iCS_Class(Company="iCanScript", Package="Test")]
public class iCS_TestGenericFunction {
    public float myFloat;
	public string myString= "working";
    
    [iCS_Function] public iCS_TestGenericFunction() { myFloat= 0f;}
    [iCS_Function] public iCS_TestGenericFunction(char[] v) { foreach(var c in v) myString+= c; }
    [iCS_Function] public bool IsA<T>(T b) { return false; }

    public override string ToString() { return myString; }
    
    public class TestClass {
        public int x= 1;
        public int y= 2;
    }
    public enum MyEnum { Red, Blue, Yellow }; 

    [iCS_Function] public int Add(TestClass v, MyEnum e, TestClass[] boolArray) { return v.x+v.y; }
}
