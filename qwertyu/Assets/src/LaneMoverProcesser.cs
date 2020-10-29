using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneMoverProcesser : MonoBehaviour {

    public char type;
    public char lane;
    public long hitTick;
    public float fromValue;
    public float toValue;
    public long appearPosition;
    public long hitPosition;
    public AnimationCurve[] curve;

    public bool inProcess = false;
    
    LaneProcesser parentSrcComp;
    SpriteRenderer laneSR;

    void Start() {
        parentSrcComp = transform.parent.gameObject.GetComponent<LaneProcesser>();
        if (type == 'a') laneSR = transform.parent.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
    }

    void Update() {

        float time = (float)(parentSrcComp.position - appearPosition) / (hitPosition - appearPosition);

        if (inProcess) {
            if (time < 0 || time > 1) inProcess = false;
        } else {
            if (time >= 0 && time <= 1) inProcess = true;
            else return;
        }
        for (int i = curve.Length - 1; i > 0; i--) time = 1 - curve[i].Evaluate(time);

        switch (type) {
            case 'u': case 'v': case 'w': {
                Vector3 parentEulerAngles = new Vector3(0, 0, 0);
                switch (type) {
                    case 'u': parentEulerAngles.x = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'v': parentEulerAngles.y = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'w': parentEulerAngles.z = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                }
                if (parentSrcComp.alreadyRotated) transform.parent.rotation *= Quaternion.Euler(parentEulerAngles);
                else {
                    transform.parent.rotation = Quaternion.Euler(parentSrcComp.uniqueEulerAngles) * Quaternion.Euler(parentEulerAngles);
                    parentSrcComp.alreadyRotated = true;
                }
                if (!inProcess) {
                    switch (type) {
                        case 'u': parentSrcComp.uniqueEulerAngles.x = parentEulerAngles.x + 90; break;
                        case 'v': parentSrcComp.uniqueEulerAngles.y = parentEulerAngles.y; break;
                        case 'w': parentSrcComp.uniqueEulerAngles.z = parentEulerAngles.z; break;
                    }
                }
            } break;
            case 'x': case 'y': case 'z': {
                Vector3 parentPosition = transform.parent.localPosition;
                switch (type) {
                    case 'x': parentPosition.x = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'y': parentPosition.y = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                    case 'z': parentPosition.z = fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time)); break;
                }
                transform.parent.localPosition = parentPosition;
            } break;
            case 'a': {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", new Color(1, 1, 1, fromValue + (toValue - fromValue) * (1 - curve[0].Evaluate(time))));
                laneSR.SetPropertyBlock(mpb);
            } break;
        }
    }
}
