using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoProcesser : MonoBehaviour {

    [Range(0, 1)] public float progress = 0;
    [Range(0, 10000)] public int combo = 0;
    public string title = "";
    public string author = "";
    [Range(0, 10000000)] public double score = 0;
    public bool fullCombo = false;
    public bool allPerfect = false;
    [Range(-1, 1)] public float effectYpos;

    double smoothedScore = 0;

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

        effectYpos = (float)Math.Sin(GameMaster.gameMasterTime / 20000000f * Math.PI);

        progressbarTransforms[0].localPosition = new Vector3(0, 0, 15 * (1 - progress));
        progressbarTransforms[1].localPosition = new Vector3(0, 0, -15 * progress);
        progressbarTransforms[0].localScale = new Vector3(1, 0, 3 * progress);
        progressbarTransforms[1].localScale = new Vector3(1, 0, 3 * (1f - progress));

        progressbarTransforms[0].GetComponent<MeshRenderer>().material.color = new Color32(208, 208, 255, (byte)(64 + effectYpos * 32));
        progressbarTransforms[1].GetComponent<MeshRenderer>().material.color = new Color32(208, 208, 255, (byte)(16 + effectYpos * 8));
        
        fcapTransforms[0].GetComponent<MeshRenderer>().material.color = new Color32(136, 238, 255, (byte)((96 + effectYpos * 16) * (fullCombo ? 1 : 0.2)));
        fcapTransforms[1].GetComponent<MeshRenderer>().material.color = new Color32(255, 255, 160, (byte)((96 + effectYpos * 16) * (allPerfect ? 1 : 0.2)));

        textTransforms[0].localPosition = new Vector3(0, -20, effectYpos * 1.5f);
        textTransforms[1].localPosition = new Vector3(0, -10, 55 + effectYpos * 1.5f);
        textTransforms[2].localPosition = new Vector3(-160, 0, 7.5f + effectYpos * 2);
        textTransforms[3].localPosition = new Vector3(-160, 0, -10 + effectYpos * 2);
        textTransforms[4].localPosition = new Vector3(160, 0, effectYpos * 2);
        
        textTransforms[0].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(48 + effectYpos * 16));
        textTransforms[1].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(48 + effectYpos * 16));
        textTransforms[2].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(32 + effectYpos * 16));
        textTransforms[3].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(32 + effectYpos * 16));
        textTransforms[4].GetComponent<TextMesh>().color = new Color32(208, 208, 255, (byte)(32 + effectYpos * 16));
        
        textTransforms[0].GetComponent<TextMesh>().text = combo >= 3 ? combo.ToString() : "";
        textTransforms[1].GetComponent<TextMesh>().text = combo >= 3 ? "COMBO" : "";
        textTransforms[2].GetComponent<TextMesh>().text = title;
        textTransforms[3].GetComponent<TextMesh>().text = author;

        smoothedScore += (score - smoothedScore) / 10;
        int ceiledScore = (int)Math.Round(smoothedScore);
        textTransforms[4].GetComponent<TextMesh>().text = "00000000".Substring(0, Math.Max(8 - ceiledScore.ToString().Length, 0)) + ceiledScore.ToString();
    }
}
