using UnityEngine;
using System;
using System.Collections;

[iCS_Class(Company="iCanScript")]
public static class iCS_TypeCasts {
    // To Bool.
    [iCS_TypeCast] public static bool   ToBool(int v)       { return v != 0; }
    [iCS_TypeCast] public static bool   ToBool(float v)     { return Math3D.IsNotZero(v); }
    [iCS_TypeCast] public static bool   ToBool(string s)    { return s != null && s != ""; }
    [iCS_TypeCast] public static bool   ToBool(Vector2 v)   { return Math3D.IsNotZero(v); }
    [iCS_TypeCast] public static bool   ToBool(Vector3 v)   { return Math3D.IsNotZero(v); }
    [iCS_TypeCast] public static bool   ToBool(Vector4 v)   { return Math3D.IsNotZero(v); }
    
    // To Int.
    [iCS_TypeCast] public static int    ToInt(bool v)   { return v ? 1 : 0; }
    [iCS_TypeCast] public static int    ToInt(float v)  { return (int)v; }

    // To Float.
    [iCS_TypeCast] public static float  ToFloat(bool v) { return v ? 1f : 0f; }
    [iCS_TypeCast] public static float  ToFloat(int v)  { return (float)v; }
    
    // To String.
	[iCS_TypeCast] public static string ToString(object v)      { return v.ToString(); }
    [iCS_TypeCast] public static string ToString(bool v)        { return v ? "true" : "false"; }
    [iCS_TypeCast] public static string ToString(int v)         { return v.ToString(); }
    [iCS_TypeCast] public static string ToString(float v)       { return v.ToString(); }
    [iCS_TypeCast] public static string ToString(Vector2 v)     { return v.ToString(); }
    [iCS_TypeCast] public static string ToString(Vector3 v)     { return v.ToString(); }
    [iCS_TypeCast] public static string ToString(Vector4 v)     { return v.ToString(); }

    // To Vector2.
    [iCS_TypeCast] public static Vector2 ToVector2(Vector3 v)   { return v; }
    [iCS_TypeCast] public static Vector2 ToVector2(Vector4 v)   { return v; }

    // To Vector3.
    [iCS_TypeCast] public static Vector3 ToVector3(Vector2 v)   { return v; }
    [iCS_TypeCast] public static Vector3 ToVector3(Vector4 v)   { return v; }

    // To Vector4.
    [iCS_TypeCast] public static Vector4 ToVector4(Vector2 v)   { return v; }
    [iCS_TypeCast] public static Vector4 ToVector4(Vector3 v)   { return v; }

    // To ... (usefull automatic conversions)
    [iCS_TypeCast] public static Animation           ToAnimation(GameObject go)           { return go.animation; }
    [iCS_TypeCast] public static AudioSource         ToAudioSource(GameObject go)         { return go.audio; }
    [iCS_TypeCast] public static Camera              ToCamera(GameObject go)              { return go.camera; }
    [iCS_TypeCast] public static Collider            ToCollider(GameObject go)            { return go.collider; }
    [iCS_TypeCast] public static Collider2D          ToCollider2D(GameObject go)          { return go.collider2D; }
    [iCS_TypeCast] public static ConstantForce       ToConstantForce(GameObject go)       { return go.constantForce; }
    [iCS_TypeCast] public static GUIText             ToGUIText(GameObject go)             { return go.guiText; }
    [iCS_TypeCast] public static GUITexture          ToGUITexture(GameObject go)          { return go.guiTexture; }
    [iCS_TypeCast] public static HingeJoint          ToHingeJoint(GameObject go)          { return go.hingeJoint; }
    [iCS_TypeCast] public static Light               ToLight(GameObject go)               { return go.light; }
    [iCS_TypeCast] public static NetworkView         ToNetworkView(GameObject go)         { return go.networkView; }
    [iCS_TypeCast] public static ParticleEmitter     ToParticleEmitter(GameObject go)     { return go.particleEmitter; }
    [iCS_TypeCast] public static Renderer            ToRenderer(GameObject go)            { return go.renderer; }
    [iCS_TypeCast] public static Rigidbody           ToRigidBody(GameObject go)           { return go.rigidbody; }
    [iCS_TypeCast] public static Rigidbody2D         ToRigidBody2D(GameObject go)         { return go.rigidbody2D; }
    [iCS_TypeCast] public static Transform           ToTransform(GameObject go)           { return go.transform; }
    [iCS_TypeCast] public static CharacterController ToCharacterController(GameObject go) { return go.GetComponent(typeof(CharacterController)) as CharacterController; }
    [iCS_TypeCast] public static MonoBehaviour       ToCharacterController(GameObject go) { return go.GetComponent(typeof(MonoBehaviour)) as MonoBehaviour; }
    
    // To GameObject from components
    [iCS_TypeCast] public static GameObject          ToGameObject(Component c)            { return c.gameObject; }
    [iCS_TypeCast] public static GameObject          ToGameObject(Transform c)            { return c.gameObject; }
}


