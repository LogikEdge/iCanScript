using UnityEngine;
using System.Collections;

namespace DisruptiveSoftware {

public static class JSONPrettyPrint {
    // ======================================================================
    // Constants
    // ----------------------------------------------------------------------
    const string kTab= "  ";
    
    // ----------------------------------------------------------------------
    public static string Print(string encoded, int lineWidth= 132) {
        int indent= 0;
        string result= "";
        for(int i= 0; i < encoded.Length; ++i) {
            char c= encoded[i];
            switch(c) {
                case '[':
                case '{':
                    ++indent;
                    result+= c+"\n"+GenerateIndent(indent);
                    break;
                case ']':
                case '}':
                    --indent;
                    result+= "\n"+GenerateIndent(indent)+c;
                    break;
                case ',':
                    result+= c+"\n"+GenerateIndent(indent);
                    break;
                case '"':
                    result+= c;
                    for(++i; i < encoded.Length; ++i) {
                        c= encoded[i];
                        result+= c;
                        if(c == '"') {
                            break;
                        }
                        if(c == '\\') {
                            ++i;
                            result+= encoded[i];
                        }
                    }
                    break;
                default:
                    result+= c;
                    break;
            }
        }
        return result;
    }
    static string GenerateIndent(int indent) {
        string result= "";
        for(int i= 0; i < indent; ++i) {
            result+= kTab;
        }
        return result;
    }
}

} // namespace DisruptiveSoftware