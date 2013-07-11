using UnityEngine;

public class iCS_Aggregate : iCS_ParallelDispatcher, iCS_ISignature {
    // ======================================================================
    // Properties
    // ----------------------------------------------------------------------
    iCS_SignatureDataSource    mySignature;

    // ======================================================================
    // Creation/Destruction
    // ----------------------------------------------------------------------
    public iCS_Aggregate(iCS_Storage storage, int instanceId, int priority, int nbOfParameters= 0)
    : base(storage, instanceId, priority) {
        mySignature= new iCS_SignatureDataSource(nbOfParameters, false, true);
    }

    // ======================================================================
    // Accessors
    // ----------------------------------------------------------------------
    public object this[int idx] {
        get { return mySignature.GetParameter(idx); }
        set { mySignature.SetParameter(idx, value); }
    }
    // ======================================================================
    // IParams implementation
    // An aggregate only support value input parameters.  All other types of
    // parameters are ignored.
    // ----------------------------------------------------------------------
    public iCS_SignatureDataSource GetSignatureDataSource() {
        return mySignature;
    }
    public iCS_Action GetAction() {
        return this;
    }
    
}
