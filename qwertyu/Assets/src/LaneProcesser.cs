using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneProcesser : MonoBehaviour {

    public long position;

    [HeaderAttribute("Moving")]
    public bool alreadyMovedInThisFrame = false;
    public Vector3 uniquePosition;

    [HeaderAttribute("Rotating")]
    public bool alreadyRotatedInThisFrame = false;
    public Quaternion uniqueQuaternion;

    void Start() {
        uniquePosition = transform.localPosition;
        uniqueQuaternion = Quaternion.Euler(transform.localEulerAngles);
    }

    void Update() {
        alreadyMovedInThisFrame = false;
        alreadyRotatedInThisFrame = false;
    }
}