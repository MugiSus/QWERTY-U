using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNoteProcesser : MonoBehaviour {

    public short longNoteID;
    public bool isReversed;
    [SerializeField] float longPairPosition;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Mesh mesh;

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
    }

    void Update() {
        
        Color32 tempColor = meshRenderer.material.color;
        tempColor.a = GameMaster.longNoteInfoStorage[longNoteID].alpha;
        meshRenderer.material.color = tempColor;

        longPairPosition = (GameMaster.longNoteInfoStorage[longNoteID].startPosition - transform.localPosition.y) / 12.5f;

        Vector3 childPos = transform.GetChild(0).localPosition;
        childPos.y = longPairPosition;
        transform.GetChild(0).localPosition = childPos;

        mesh.vertices = new Vector3[] {
            new Vector3(-1, 0, 0),
            new Vector3(0, isReversed ? -1 : 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, longPairPosition, 0),
            new Vector3(0, longPairPosition + (isReversed ? -1 : 1), 0),
            new Vector3(1, longPairPosition, 0),
        };
        mesh.triangles = new int[] {3, 1, 0, 5, 2, 1, 3, 4, 1, 1, 4, 5};

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.sharedMesh = mesh;

    }
}
