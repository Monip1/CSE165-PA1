using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus;
public class HandTrack : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject mixamo;
    public Animator mixamoAnimator;
    private OVRSkeleton skeleton;
    private Transform indexTip;
    private bool initialized = false;
    // Start is called before the first frame update
    void Start()
    {
        skeleton = leftHand.GetComponent<OVRSkeleton>();
        mixamoAnimator = mixamo.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!initialized && skeleton.Bones != null) {
            foreach(var bone in skeleton.Bones) {
                if(bone.Id == OVRSkeleton.BoneId.Hand_IndexTip) {
                    indexTip = bone.Transform;
                    initialized = true;
                    break;
                }
            }
        }
        var rhand = rightHand.GetComponent<OVRHand>();
        var lhand = leftHand.GetComponent<OVRHand>();
        bool middlePinch = rhand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
        bool indexPinch = rhand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        Transform pointDir = lhand.GetPointerRayTransform();
        // pointDir.forward.y = 0;
        if(indexPinch && initialized && indexTip != null) {
            Debug.Log(indexTip.forward);
            mixamoAnimator.SetBool("isWalking", true);
            mixamo.transform.position = mixamo.transform.position + pointDir.forward * 0.05f;
        }
        else if(middlePinch && initialized && indexTip != null){
            Debug.Log(indexTip.forward);
            mixamoAnimator.SetBool("isWalking", true);
            mixamo.transform.position = mixamo.transform.position - pointDir.forward * 0.05f;
        }
        else {

            mixamoAnimator.SetBool("isWalking", false);
        }
        bool isWalkingValue = mixamoAnimator.GetBool("isWalking");
        Debug.Log("isWalking: " + isWalkingValue);
    }
}
