using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteProcesser : MonoBehaviour {
    
    public char type;
    public char lane;
    public sbyte keyNum;
    public AnimationCurve[] curve;
    public long hit;
    public long appearPosition;
    public long hitPosition;
    public short longNoteID;
    public bool isReversed;
    public bool isMultiNote;
    
    MaterialPropertyBlock mpb;
    SpriteRenderer timingSupportSR;
    SpriteRenderer noteSR;
    LaneProcesser parentSrcComp;

    void Start() {
        mpb = new MaterialPropertyBlock();
        timingSupportSR = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        noteSR = gameObject.GetComponent<SpriteRenderer>();
        parentSrcComp = transform.parent.gameObject.GetComponent<LaneProcesser>();

        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
    }

    void Update() {
        
        float time = (float)(parentSrcComp.position - appearPosition) / (hitPosition - appearPosition);
        
        if (time < 0) transform.localPosition = new Vector3(0, isReversed ? 160 : -160, 0);
        else if (time > 1) transform.localPosition = new Vector3(0, 0, 0);
        else {
            for (int i = curve.Length - 1; i > 0; i--) time = 1 - curve[i].Evaluate(time);
            transform.localPosition = new Vector3(0f, isReversed ? curve[0].Evaluate(time) * -160f : curve[0].Evaluate(time) * 160f, 0f);
        }
        
        float scale = 2f - (float)Math.Pow(1 - (hit - GameMaster.gameMasterTime) / 30000000f, 5);
        transform.GetChild(0).localScale = new Vector3(scale, scale, 1);

        float alpha = 1 - (hit - GameMaster.gameMasterTime) / 30000000f;
        
        if (alpha < 0 || alpha > 1 || time < 0 || time > 1) alpha = 0;
        mpb.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
        timingSupportSR.SetPropertyBlock(mpb);

        if (time < 0 || time > 1) mpb.SetColor("_Color", new Color(1, 1, 1, 0));
        else mpb.SetColor("_Color", new Color(1, 1, 1, 1));
        noteSR.SetPropertyBlock(mpb);
    }
}
