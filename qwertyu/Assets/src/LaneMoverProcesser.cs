using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneMoverProcesser : MonoBehaviour {

    public string type;
    public char lane;
    public long hitTick;
    public float fromValue;
    public float toValue;
    public long appearPosition;
    public long hitPosition;
    public AnimationCurve[] curve;
    
    [SerializeField] bool inProcess = false;
    [SerializeField] bool isPast = false;
    [SerializeField] Vector3 additionalVector = new Vector3(0, 0, 0);
    
    LaneProcesser parentSrcComp;
    SpriteRenderer laneSR;

    void Start() {
        parentSrcComp = transform.parent.gameObject.GetComponent<LaneProcesser>();
        if (type == "a") laneSR = transform.parent.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
    }

    void Update() {

        float time = (float)(parentSrcComp.position - appearPosition) / (hitPosition - appearPosition);

        if (inProcess) {
            if (time < 0) inProcess = false;
            else if (time > 1) {
                isPast = true;
                inProcess = false;
            } else if (isPast) isPast = false;
        } else {
            if (time >= 0 && time <= 1) inProcess = true;
            else return;
        }
        for (int i = curve.Length - 1; i > 0; i--) time = 1 - curve[i].Evaluate(time);

        switch (type) {
            case "~x": case "~y": case "~z": {
                
            } break;
            case "x": case "y": case "z": {
                if (inProcess && isPast) parentSrcComp.uniquePosition -= additionalVector;
                additionalVector = new Vector3(0, 0, 0);
                switch (type[0]) {
                    case 'x': additionalVector.x = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'y': additionalVector.y = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'z': additionalVector.z = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                }
                if (parentSrcComp.alreadyMovedInThisFrame) transform.parent.localPosition += additionalVector;
                else {
                    transform.parent.localPosition = parentSrcComp.uniquePosition + additionalVector;
                    parentSrcComp.alreadyMovedInThisFrame = true;
                }
                if (!inProcess && isPast) parentSrcComp.uniquePosition += additionalVector;
            } break;
            case "dx": case "dy": case "dz": {
                if (inProcess && isPast) parentSrcComp.uniqueQuaternion *= Quaternion.Euler(additionalVector * -1);
                additionalVector = new Vector3(0, 0, 0);
                switch (type[1]) {
                    case 'x': additionalVector.x = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'y': additionalVector.y = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'z': additionalVector.z = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                }
                if (parentSrcComp.alreadyRotatedInThisFrame) transform.parent.rotation *= Quaternion.Euler(additionalVector);
                else {
                    transform.parent.rotation = parentSrcComp.uniqueQuaternion * Quaternion.Euler(additionalVector);
                    parentSrcComp.alreadyRotatedInThisFrame = true;
                }
                if (!inProcess && isPast) parentSrcComp.uniqueQuaternion = transform.parent.rotation;
            } break;
            case "a": {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", new Color(1, 1, 1, fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time))));
                laneSR.SetPropertyBlock(mpb);
            } break;
        }
    }
}
