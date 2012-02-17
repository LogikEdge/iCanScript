using UnityEngine;
using System.Collections;

[iCS_Class(Company="iCanScript")]
public static class iCS_Math3D {
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static int     Add(int a, int b)         { return a+b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static float   Add(float a, float b)     { return a+b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static Vector2 Add(Vector2 a, Vector2 b) { return a+b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static Vector3 Add(Vector3 a, Vector3 b) { return a+b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static Vector4 Add(Vector4 a, Vector4 b) { return a+b; }

    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static int     Sub(int a, int b)         { return a-b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static float   Sub(float a, float b)     { return a-b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static Vector2 Sub(Vector2 a, Vector2 b) { return a-b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static Vector3 Sub(Vector3 a, Vector3 b) { return a-b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static Vector4 Sub(Vector4 a, Vector4 b) { return a-b; }

    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static int     Mul(int a, int b)         { return a*b; }
    [iCS_Function(Icon="iCS_CalculatorIcon.psd")] public static float   Mul(float a, float b)     { return a*b; }

    [iCS_Function(Icon="iCS_SplitIcon.psd")] public static void    FromVector(Vector2 v, out float x, out float y)                           { x= v.x; y= v.y; }
    [iCS_Function(Icon="iCS_SplitIcon.psd")] public static void    FromVector(Vector3 v, out float x, out float y, out float z)              { x= v.x; y= v.y; z= v.z; }
    [iCS_Function(Icon="iCS_SplitIcon.psd")] public static void    FromVector(Vector4 v, out float x, out float y, out float z, out float w) { x= v.x; y= v.y; z= v.z; w= v.w; }

    [iCS_Function(Icon="iCS_JoinIcon.psd")] public static Vector2 ToVector(float x, float y)                   { return new Vector2(x,y); }
    [iCS_Function(Icon="iCS_JoinIcon.psd")] public static Vector3 ToVector(float x, float y, float z)          { return new Vector3(x,y,z); }
    [iCS_Function(Icon="iCS_JoinIcon.psd")] public static Vector4 ToVector(float x, float y, float z, float w) { return new Vector4(x,y,z,w); }

    [iCS_Function] public static int     Lerp(int v1, int v2, float ratio)            { return (int)(v1+(v2-v1)*ratio); }
    [iCS_Function] public static float   Lerp(float v1, float v2, float ratio)        { return v1+(v2-v1)*ratio; }
    [iCS_Function] public static Vector2 Lerp(Vector2 v1, Vector2 v2, float ratio)    { return v1+(v2-v1)*ratio; }
    [iCS_Function] public static Vector3 Lerp(Vector3 v1, Vector3 v2, float ratio)    { return v1+(v2-v1)*ratio; }
    [iCS_Function] public static Vector4 Lerp(Vector4 v1, Vector4 v2, float ratio)    { return v1+(v2-v1)*ratio; }
    
    [iCS_Function(Name="Random",Icon="iCS_RandomIcon.png")] public static float   Random(float scale)         { return scale*UnityEngine.Random.value; }
    [iCS_Function(Name="Random",Icon="iCS_RandomIcon.png")] public static Vector2 RandomVector2(float scale)  { return scale*UnityEngine.Random.insideUnitCircle; }
    [iCS_Function(Name="Random",Icon="iCS_RandomIcon.png")] public static Vector3 RandomVector3(float scale)  { return scale*UnityEngine.Random.insideUnitSphere; }
    
    [iCS_Function] public static Vector2 ScaleVector(float scale, Vector2 v) { return scale*v; }
    [iCS_Function] public static Vector3 ScaleVector(float scale, Vector3 v) { return scale*v; }
    [iCS_Function] public static Vector4 ScaleVector(float scale, Vector4 v) { return scale*v; }

    [iCS_Function] public static Vector2 Scale2Vector(float s1, float s2, Vector2 v) { return s1*s2*v; }
    [iCS_Function] public static Vector3 Scale2Vector(float s1, float s2, Vector3 v) { return s1*s2*v; }
    [iCS_Function] public static Vector4 Scale2Vector(float s1, float s2, Vector4 v) { return s1*s2*v; }
    
    [iCS_Function(ToolTip="Returns the normalized cross product.")]
    public static Vector3 NormalizedCross(Vector3 v1, Vector3 v2) { return Vector3.Cross(v1,v2).normalized; }
}
