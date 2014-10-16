using UnityEngine;
using System.Collections;

public partial class iCS_IStorage {
    // ----------------------------------------------------------------------
    // Performs all sanity checks.
    public void SanityCheck() {
	    SanityCheck_EditorEngineContainerCoherency();
		SanityCheck_ParameterIndexes();

    }

    // ----------------------------------------------------------------------
    // Validates the editor/engine container coherency.
    void SanityCheck_EditorEngineContainerCoherency() {
        if(EditorObjects.Count != EngineObjects.Count) {
            Debug.LogWarning("iCanScript: Editor/Engine contains are not of same size !!!\nEditorObjects size: "+
                             EditorObjects.Count+" EngineObjects size: "+EngineObjects.Count);
        }
    }

    // ----------------------------------------------------------------------
	void SanityCheck_ParameterIndexes() {
		ForEachNode(
			node=> {
				var ports= BuildFilteredListOfChildren(p=> p.IsPort, node);
				if(ports.Length == 0) return;
				ports= SortPortsOnIndex(ports);
				// Check parameters
				int i= 0;
				int portIndex= ports[i].ParameterIndex;
				int parametersStart= (int)iCS_ParameterIndex.ParametersStart;
				int parametersEnd  = (int)iCS_ParameterIndex.ParametersEnd;
				if(portIndex >= parametersStart && portIndex <= parametersEnd) {
					bool isFixPort= true;
					int expectedParameterIndex= parametersStart;
					for(; i < ports.Length; ++i, ++expectedParameterIndex) {
						portIndex= ports[i].ParameterIndex;
						if(portIndex < parametersStart || portIndex > parametersEnd) break;
						if(portIndex != expectedParameterIndex) {
							Debug.LogWarning("iCanScript: Expected port index was: "+expectedParameterIndex+" actual is: "+portIndex+" => "+ports[i].Name);
						}
						if(isFixPort) {
							if(!ports[i].IsFixDataPort) {
								if(ports[i].IsDynamicDataPort || ports[i].IsProposedDataPort) {
									isFixPort= false;
								}
							}
						}
						if(!isFixPort) {
							if(!ports[i].IsDynamicDataPort && !ports[i].IsProposedDataPort) {
								Debug.LogWarning("iCanScript: Invalid parameter object type: "+ports[i].ObjectType+" => "+ports[i].Name);
							}
								
						}
					}
				}
				// Check return
				// Check 'this'
				// Check 'outThis'
				// Check trigger
				// Check enables
			}
		);
	}
	
    // ----------------------------------------------------------------------
    // Validates that animation has been properly applied.
    void SanityCheck_Animation() {
//        ForEach(
//            obj=> {
//                var animation= obj.AnimatedPosition;
//                if(obj.IsVisible) {
//                    if(Math3D.IsNotEqual(obj.GlobalRect, animation.TargetValue)) {
//                        Debug.LogWarning("iCanScript: Animation was not properly applied for: "+obj.Name+" with id: "+obj.InstanceId);                        
//                    }
//                } else {
//                    float area= animation.TargetValue.width*animation.TargetValue.height;
//                    if(Math3D.IsNotZero(area)) {
//                        Debug.LogWarning("iCanScript: Shrink animation was not properly applied for: "+obj.Name+" with id: "+obj.InstanceId);
//                    }
//                }
//            }
//        );
    }
}
