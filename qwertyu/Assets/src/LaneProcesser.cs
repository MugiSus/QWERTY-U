using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneProcesser : MonoBehaviour {

    public long position;
    public bool alreadyRotated = false;
    public Vector3 uniqueEulerAngles = new Vector3(90, 0, 0);

    void Start() {

    }

    void Update() {
        alreadyRotated = false;
    }

}
