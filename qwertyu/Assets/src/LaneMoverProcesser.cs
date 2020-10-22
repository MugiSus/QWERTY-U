using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneMoverProcesser : MonoBehaviour {

    public char type;
    public char lane;
    public long hit;
    public float fromValue;
    public float toValue;
    public long appearPosition;
    public long hitPosition;
    public AnimationCurve[] curve;
    
    LaneProcesser parentSrcComp;
    SpriteRenderer laneSR;

    void Start() {
        parentSrcComp = transform.parent.gameObject.GetComponent<LaneProcesser>();
        laneSR = transform.parent.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
    }

    void Update() {

        float time = (float)(parentSrcComp.position - appearPosition) / (hitPosition - appearPosition);

        if (time < 0) return;
        else if (time > 1) return;
        for (int i = curve.Length - 1; i > 0; i--) time = 1 - curve[i].Evaluate(time);

        switch (type) {
            case 'x': case 'y': case 'z': {
                Vector3 parentPosition = transform.parent.localPosition;
                switch (type) {
                    case 'x': parentPosition.x = fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue); break;
                    case 'y': parentPosition.y = fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue); break;
                    case 'z': parentPosition.z = fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue); break;
                }
                transform.parent.localPosition = parentPosition;
            } break;
            case 'u': case 'v': case 'w': {
                Vector3 parentEulerAngles = transform.parent.localEulerAngles;
                switch (type) {
                    case 'u': parentEulerAngles.x = fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue); break;
                    case 'v': parentEulerAngles.y = fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue); break;
                    case 'w': parentEulerAngles.z = fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue); break;
                }
                transform.parent.localEulerAngles = parentEulerAngles;
            } break;
            case 'a': {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", new Color(1, 1, 1, fromValue + (1 - curve[0].Evaluate(time)) * (toValue - fromValue)));
                laneSR.SetPropertyBlock(mpb);
            } break;
        }
    }
}
