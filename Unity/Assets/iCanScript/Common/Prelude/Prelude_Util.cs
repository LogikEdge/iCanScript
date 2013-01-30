using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static partial class Prelude {
    // ======================================================================
    //  List/Array
    // ----------------------------------------------------------------------
    // Returns the length of a list or array.
    public static int length<T>(T[] a)      { return a.Length; }
    public static int length<T>(List<T> l)  { return l.Count; }
    
    // ======================================================================
    //  C# Currying fix
    // ----------------------------------------------------------------------
    // This function can fixes a problem with C# implementation of the
    // Prelude by allowing a partial function of type 'f(a)(b,c)' to be
    // converted to 'f(a)(b)(c)' as follows 'fix(f)(b,c)'
    public static C fix<A,B,C>(Func<A,Func<B,C>> f, A a, B b) {
         return f(a)(b);
    }
    public static Func<A,B,C>  fix<A,B,C>(Func<A,Func<B,C>> f) {
        return delegate(A a, B b) { return fix(f,a,b); };
    }

    // ----------------------------------------------------------------------
    public static Func<B,C> function<A,B,C>(Func<A,B,C> f, A a) {
        return delegate(B b) { return f(a,b); };
    }
    public static Func<C,D> function<A,B,C,D>(Func<A,B,C,D> f, A a, B b) {
        return delegate(C c) {  return f(a,b,c); };
    }
    public static Func<B,Func<C,D>> function<A,B,C,D>(Func<A,B,C,D> f, A a) {
        return delegate(B b) { return function<A,B,C,D>(f,a,b); };
    }
}