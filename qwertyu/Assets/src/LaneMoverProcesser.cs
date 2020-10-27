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
            case 'x': {
                Vector3 parentPosition = transform.parent.localPosition;
                parentPosition.x = Mathf.Lerp(fromValue, toValue, 1 - curve[0].Evaluate(time)); break;
                transform.parent.localPosition = parentPosition;
            } break;
            case 'y': {
                Vector3 parentPosition = transform.parent.localPosition;
                parentPosition.y = Mathf.Lerp(fromValue, toValue, 1 - curve[0].Evaluate(time)); break;
                transform.parent.localPosition = parentPosition;
            } break;
            case 'z': {
                Vector3 parentPosition = transform.parent.localPosition;
                parentPosition.z = Mathf.Lerp(fromValue, toValue, 1 - curve[0].Evaluate(time)); break;
                transform.parent.localPosition = parentPosition;
            } break;
            case 'd': {
                transform.parent.localEulerAngles = new Vector3(90, Mathf.Lerp(fromValue, toValue, 1 - curve[0].Evaluate(time)), 0);
            } break;
            case 'a': {
                var mpb = new MaterialPropertyBlock();
                mpb.SetColor("_Color", new Color(1, 1, 1, Mathf.Lerp(fromValue, toValue, 1 - curve[0].Evaluate(time))));
                laneSR.SetPropertyBlock(mpb);
            } break;
        }
    }
}
