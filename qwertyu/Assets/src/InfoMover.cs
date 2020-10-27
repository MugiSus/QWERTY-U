using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoMover : MonoBehaviour {

    public double progress;

    Transform[] progressbarTransforms;
    Transform[] fcapTransforms;
    Transform[] textTransforms;

    void Start() {

        progressbarTransforms = new Transform[] {
            transform.Find("progressbar/reached"),
            transform.Find("progressbar/unreached"),
        };

        fcapTransforms = new Transform[] {
            transform.Find("fcap/fullcombo"),
            transform.Find("fcap/allperfect"),
        };
        
        textTransforms = new Transform[] {
            transform.Find("texts/combo_num"),
            transform.Find("texts/combo_text"),
            transform.Find("texts/title"),
            transform.Find("texts/author"),
            transform.Find("texts/score")
        };
    }

    void Update() {

        double nowTickSin = Math.Sin(DateTime.Now.Ticks / 20000000d * Math.PI);

        progress = 0;

        progressbarTransforms[0].position = new Vector3(-160, 0, 15f * (1f - (float)progress));
        progressbarTransforms[1].position = new Vector3(-160, 0, -15f * (float)progress);
        progressbarTransforms[0].localScale = new Vector3(1, 0, 3 * (float)progress);
        progressbarTransforms[1].localScale = new Vector3(1, 0, 3 * (1f - (float)progress));

        progressbarTransforms[0].GetComponent<MeshRenderer>().material.color = new Color32(208, 208, 255, (byte)(64d + nowTickSin * 32d));
        progressbarTransforms[1].GetComponent<MeshRenderer>().material.color = new Color32(208, 208, 255, (byte)(16d + nowTickSin * 8d));
        
        fcapTransforms[0].GetComponent<MeshRenderer>().material.color = new Color32(136, 238, 255, (byte)(64d + nowTickSin * 32d));
        fcapTransforms[1].GetComponent<MeshRenderer>().material.color = new Color32(255, 255, 160, (byte)(64d + nowTickSin * 32d));

        textTransforms[0].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(48d + nowTickSin * 16d));
        textTransforms[1].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(48d + nowTickSin * 16d));
        textTransforms[2].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(32d + nowTickSin * 16d));
        textTransforms[3].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(32d + nowTickSin * 16d));
        textTransforms[4].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(32d + nowTickSin * 16d));

        textTransforms[0].position = new Vector3(0, -50, (float)nowTickSin * 1.5f);
        textTransforms[1].position = new Vector3(0, -40, 55f + (float)nowTickSin * 1.5f);
        textTransforms[2].position = new Vector3(-160, -30, 7.5f + (float)nowTickSin * 2f);
        textTransforms[3].position = new Vector3(-160, -30, -10f + (float)nowTickSin * 2f);
        textTransforms[4].position = new Vector3(160, -30, (float)nowTickSin * 2f);
    }
}
