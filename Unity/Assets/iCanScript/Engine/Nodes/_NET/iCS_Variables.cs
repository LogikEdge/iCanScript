using UnityEngine;

[iCS_Class(Company="NET",Icon="iCS_LibraryIcon_32x32.png")]
public static class iCS_Variables {
    [iCS_Function(Name="bool")]      public static bool      _bool  (bool value)    { return value; }
    [iCS_Function(Name="int")]       public static int       _int   (int value)     { return value; }
    [iCS_Function(Name="float")]     public static float     _float (float value)   { return value; }
    [iCS_Function(Name="string")]    public static string    _string(string value)  { return value; }
}


[iCS_Class(Company="iCanScript",Library="Variables")]
public struct Bool {
    bool myValue;
    
    public bool Value {
        [iCS_Function] get { return myValue; }
        [iCS_Function] set { myValue= value; }
    }
    public bool Not {
        [iCS_Function] get { return !myValue; }
    }
    
    [iCS_Function] public Bool(bool init= false) { myValue= init; }
    [iCS_Function] public bool And(bool v) { return myValue & v; }
    [iCS_Function] public bool Or(bool v)  { return myValue | v; }
    [iCS_Function] public bool Xor(bool v) { return myValue ^ v; }
}

[iCS_Class(Company="iCanScript",Library="Variables")]
public struct Int {
    int myValue;
    
    public int Value {
        [iCS_Function] get { return myValue; }
        [iCS_Function] set { myValue= value; }
    }
    public int Negate {
        [iCS_Function] get { return -myValue; }
    }
    public int Abs {
        [iCS_Function] get { return myValue < 0 ? -myValue : myValue; }
    }
    public int Sign {
        [iCS_Function] get { return myValue < 0 ? -1 : 1; }
    }
    
    [iCS_Function] public Int(int init= 0) { myValue= init; }
    [iCS_Function(Return="a+b")] public int Add(int b) { return myValue+b; }
    [iCS_Function(Return="a-b")] public int Sub(int b) { return myValue-b; }
    [iCS_Function(Return="a*b")] public int Mul(int b) { return myValue*b; }
    [iCS_Function(Return="a/b")] public int Div(int b) { return myValue/b; }
}

[iCS_Class(Company="iCanScript",Library="Variables")]
public struct Float {
    float myValue;
    
    public float Value {
        [iCS_Function] get { return myValue; }
        [iCS_Function] set { myValue= value; }
    }
    public float Negate {
        [iCS_Function] get { return -myValue; }
    }
    public float Abs {
        [iCS_Function] get { return Mathf.Abs(myValue); }
    }
    public float Sign {
        [iCS_Function] get { return Mathf.Sign(myValue); }
    }
    
    [iCS_Function] public Float(float init= 0f) { myValue= init; }
    [iCS_Function(Return="a+b")] public float Add(float b) { return myValue+b; }
    [iCS_Function(Return="a-b")] public float Sub(float b) { return myValue-b; }
    [iCS_Function(Return="a*b")] public float Mul(float b) { return myValue*b; }
    [iCS_Function(Return="a/b")] public float Div(float b) { return myValue/b; }
}
