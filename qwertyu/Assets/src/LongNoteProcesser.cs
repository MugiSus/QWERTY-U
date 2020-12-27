using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNoteProcesser : MonoBehaviour {

    public short longNoteID;
    public bool isReversed;
    [SerializeField] float longTermPosition;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Mesh mesh;

    GameMaster.LongNoteInfo longNoteInfo;

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        longNoteInfo = GameMaster.longNoteInfoStorage[longNoteID];
    }

    void Update() {
        
        Color32 tempColor = meshRenderer.material.color;
        tempColor.a = Math.Max(longNoteInfo.startAlpha, longNoteInfo.endAlpha);
        meshRenderer.material.color = tempColor;

        longTermPosition = (longNoteInfo.startNotePosition - transform.localPosition.y) / 12.5f;

        //transform.localPosition = new Vector3(0, -longTermPosition, 0);

        mesh.vertices = new Vector3[] {
            new Vector3(-1, 0, 0),
            new Vector3(0, isReversed ? -1 : 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, longTermPosition, 0),
            new Vector3(0, longTermPosition + (isReversed ? -1 : 1), 0),
            new Vector3(1, longTermPosition, 0),
        };
        mesh.triangles = new int[] {3, 1, 0, 5, 2, 1, 3, 4, 1, 1, 4, 5};

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.sharedMesh = mesh;

    }
}
