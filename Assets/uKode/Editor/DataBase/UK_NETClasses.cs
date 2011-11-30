using UnityEngine;
using System;
using System.IO;
using System.Collections;

public static class UK_NETClasses {
    public static void PopulateDataBase() {
        DecodeNETClassInfo(typeof(string));
        DecodeNETClassInfo(typeof(char));
        DecodeNETClassInfo(typeof(Array));
        DecodeNETClassInfo(typeof(Path));
    }
    // ----------------------------------------------------------------------
    public static void DecodeNETClassInfo(Type type) {
        UK_Reflection.DecodeClassInfo(type, "NET", type.Name, type.Name, ".NET class "+type.Name, null, true);
    }

}
