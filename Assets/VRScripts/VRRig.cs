using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void Map()
    {
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class VRRig : MonoBehaviour
{
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand; 
    
    public Transform headConstraint;
    private Vector3 headBodyOffset;

    public int turnSmoothness;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = headConstraint.position + headBodyOffset;
        transform.forward =  Vector3.Lerp(transform.forward,
            Vector3.ProjectOnPlane(headConstraint.up,Vector3.up).normalized,Time.deltaTime * turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();

    }
}
