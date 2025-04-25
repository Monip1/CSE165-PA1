using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class NewBehaviourScript : MonoBehaviour
{
    float alpha = 4.0f;
    public Dropdown spawnDropdown;
    public Button spawnButton;
    public GameObject firstObj;
    public GameObject secondObj;

    public GameObject leftController;
    private Material orig;
    public GameObject rightController;
    public GameObject Camera;

    public GameObject XRRig;

    
    public InputActionProperty leftXAxis;
    public InputActionProperty leftYAxis;

    public InputActionProperty XButton;
    
    public InputActionProperty leftTrigger;
    public InputActionProperty rightTrigger;
    // public InputActionProperty trigger;


    public InputActionProperty gripRotation;
    public LineRenderer leftLineRenderer;
    public LineRenderer rightLineRenderer;
    public LineRenderer cameraLineRenderer;
    public InputActionProperty grip;

    public InputActionProperty leftGrip;
    public InputActionProperty rightGrip;
    
    // public InputActionProperty xAxis;
    
    // public InputActionProperty yAxis;

    // public GameObject controller;
    public GameObject tent;
    public GameObject player;
    public Vector3 tentCenter;
    public bool isLeftTriggerPressed;

    public bool isRightTriggerPressed;
    public bool isLeftGripPressed;
    public bool isRightGripPressed;

    float leftTriggerVal;
    float rightTriggerVal;
    float leftGripVal;
    float rightGripVal;
    // Start is called before the first frame update
    public Collider collider;
    public Transform highlight;
    public Material highlightMaterial;

    public bool somethingSelected = false;
    public bool spawnObj = false;

    float maxHandRayDist = 10.0f;
    float maxTeleportDist = 5.0f;

    int ColliderID;
    int stareCounter = 0;
    int stareWaitCount = 150;

    int count = 0;
    Quaternion startQuat;
    public string objectToSpawn;
    void Start()
    {
        if(spawnDropdown == null) {
            spawnDropdown = GetComponent<Dropdown>();
        }
        spawnButton.onClick.AddListener(TaskOnClick);
        highlight = null;
        collider = null;
        if(leftController != null) {
            Color color = Color.red;
            leftLineRenderer = leftController.GetComponent<LineRenderer>();
            if(leftLineRenderer == null) {
                leftLineRenderer = leftController.AddComponent<LineRenderer>();
            }
            leftLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            leftLineRenderer.startColor = color;
            leftLineRenderer.endColor = Color.green;
            leftLineRenderer.startWidth = 0.01f;
            leftLineRenderer.endWidth = 0.01f;
            leftLineRenderer.positionCount = 2;

        }
        if(rightController != null) {
            Color color = Color.red;
            rightLineRenderer = rightController.GetComponent<LineRenderer>();
            if(rightLineRenderer == null) {
                rightLineRenderer = rightController.AddComponent<LineRenderer>();
            }
            rightLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rightLineRenderer.startColor = color;
            rightLineRenderer.endColor = Color.green;
            rightLineRenderer.startWidth = 0.1f;
            rightLineRenderer.endWidth = 0.1f;
            rightLineRenderer.positionCount = 2;

        }
        if(Camera != null) {
            Color color = Color.blue;
            cameraLineRenderer = Camera.GetComponent<LineRenderer>();
            if(cameraLineRenderer == null) {
                cameraLineRenderer = Camera.AddComponent<LineRenderer>();
            }
            cameraLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            cameraLineRenderer.startColor = color;
            cameraLineRenderer.endColor = Color.green;
            cameraLineRenderer.startWidth = 0.25f;
            cameraLineRenderer.endWidth = 0.25f;
            cameraLineRenderer.positionCount = 2;

        }
        if(tent != null && player != null) {
            Renderer tentRend = tent.GetComponent<Renderer>(); 
            if(tentRend != null) {
                tentCenter = tentRend.bounds.center;
            }
            else {
                tentCenter = tent.transform.position;
            }
            Debug.Log(tentCenter);  
            player.transform.position = tentCenter;
        }
    }

    // Update is called once per frame
    void Update()
    {
        spawnDropdown.onValueChanged.AddListener(delegate {
            objectToSpawn = DropChanged(spawnDropdown);
        });
        // Debug.Log(eyePosition.action.ReadValue<Vector3>());
        Quaternion gripRotationVal = gripRotation.action.ReadValue<Quaternion>();
        leftTriggerVal = leftTrigger.action.ReadValue<float>();
        rightTriggerVal = rightTrigger.action.ReadValue<float>();
        leftGripVal = leftGrip.action.ReadValue<float>();
        rightGripVal = rightGrip.action.ReadValue<float>();
        // float xAxis = xAxis.action.ReadValue<float>();
        Vector3 leftPosition = leftController.transform.position;
        Vector3 leftForward = leftController.transform.forward;
        Vector3 leftEndPoint = leftPosition + maxHandRayDist * leftForward;

        Vector3 rightPosition = rightController.transform.position;
        Vector3 rightForward = rightController.transform.forward;
        Vector3 rightEndPoint = rightPosition + maxHandRayDist * rightForward;



        RaycastHit hit;

        if (rightLineRenderer != null && rightController != null) {
            rightLineRenderer.SetPosition(0, rightPosition);
            if(Physics.Raycast(rightPosition, rightForward, out hit,  maxHandRayDist)){
                rightLineRenderer.SetPosition(1, hit.point);
            }
            else{

                rightLineRenderer.SetPosition(1, rightEndPoint);
            }
        }
        if (leftLineRenderer != null && leftController != null) {
            leftLineRenderer.SetPosition(0, leftPosition);
            if(Physics.Raycast(leftPosition, leftForward, out hit,  maxHandRayDist)){
                
                leftLineRenderer.SetPosition(1, hit.point);
            }
            else{

                leftLineRenderer.SetPosition(1, leftEndPoint);
        
            }
        }
        Vector3 cameraPosition = Camera.transform.position;
        Vector3 cameraForward = Camera.transform.forward;
        Vector3 cameraEndPoint = cameraPosition + alpha * cameraForward;

        if (cameraLineRenderer != null && Camera != null) {
            // cameraLineRenderer.SetPosition(0, cameraPosition);
            if(Physics.Raycast(cameraPosition, cameraForward, out hit,  maxHandRayDist)){
                cameraLineRenderer.SetPosition(0, hit.point - 0.15f * cameraForward);
                cameraLineRenderer.SetPosition(1, hit.point);
            }
            else{
                cameraLineRenderer.SetPosition(0, cameraEndPoint- 0.15f*cameraForward);
                cameraLineRenderer.SetPosition(1, cameraEndPoint);
            }
        }




        UpdateSelect(leftPosition, leftForward, gripRotationVal);
        UpdateTravel(rightPosition, rightForward);
        UpdateSpawn(leftPosition, leftForward, leftEndPoint, rightPosition, rightForward, rightEndPoint, gripRotationVal);

        // Vector3 shiftPlayerPosition = new Vector3(xAxis, 0.0f, yAxis);
        // player.transform.position += shiftPlayerPosition;
    }
    public void TaskOnClick() {
        spawnObj = true;
    }
    public string DropChanged(Dropdown change) {
        return change.captionText.text;
    }
    void UpdateSelect(Vector3 position, Vector3 forward, Quaternion gripRotationVal){

        RaycastHit hit;
        Vector3 cameraPosition = Camera.transform.position;
        Vector3 cameraForward = Camera.transform.forward;
        Vector3 cameraEndPoint = cameraPosition + alpha * cameraForward;

        if (cameraLineRenderer != null && Camera != null) {
            count ++;
            // cameraLineRenderer.SetPosition(0, cameraPosition);
            if(Physics.Raycast(cameraPosition, cameraForward, out hit,  maxHandRayDist)){
                // if(count %2 == 1){
                //     cameraLineRenderer.SetPosition(0, hit.point - 0.1f * cameraForward);
                //     cameraLineRenderer.SetPosition(1, hit.point);
                // }
                


                if(ColliderID == hit.colliderInstanceID){
                    stareCounter++;
                }
                else{
                    stareCounter = 0;
                    ColliderID = hit.colliderInstanceID;
                }
                if(stareCounter >= stareWaitCount && !somethingSelected){
                    collider = hit.collider;
                    highlight = hit.transform;
                    if(highlight.gameObject.tag.Equals("selectable")) {
                        orig = highlight.GetComponent<MeshRenderer>().material;
                        highlight.GetComponent<MeshRenderer>().material = highlightMaterial;
                        somethingSelected = true;
                    }
                    stareCounter = 0;
                }
                else if (stareCounter >= stareWaitCount && somethingSelected){
                    collider = null;
                    if(somethingSelected){
                        highlight.GetComponent<MeshRenderer>().material = orig;
                        orig = null;
                        somethingSelected = false;
                    }
                    stareCounter = 0;
                }
            }
            else{
                // if(count %2 == 1){
                //     cameraLineRenderer.SetPosition(0, cameraEndPoint- 0.05f*cameraForward);
                //     cameraLineRenderer.SetPosition(1, cameraEndPoint);
                // }
            }
        }

        if(somethingSelected) {
            Vector3 leftEndPoint = position + alpha * forward;
            highlight.position = leftEndPoint;
            highlight.rotation = gripRotationVal;
            if(leftGripVal >  0.7f) {
                highlight.localScale += new Vector3(0.1f, 0.1f, 0.1f);
            }
            else if(rightGripVal >  0.7f) {
                highlight.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
        if(leftTriggerVal > 0.7f && !isLeftTriggerPressed){
            // Debug.Log("Left Trigger Val: " + leftTriggerVal);
            isLeftTriggerPressed = true;


            // RaycastHit hit;
            if(Physics.Raycast(position, forward, out hit,  maxHandRayDist) && !somethingSelected){
                collider = hit.collider;
                highlight = hit.transform;
                if(highlight.gameObject.tag.Equals("selectable")) {
                    orig = highlight.GetComponent<MeshRenderer>().material;
                    highlight.GetComponent<MeshRenderer>().material = highlightMaterial;
                    somethingSelected = true;
                }
            }
            else {
                
            }
        }
        else if(leftTriggerVal < 0.1f && isLeftTriggerPressed){

            isLeftTriggerPressed = false;
            collider = null;
            if(somethingSelected){
                highlight.GetComponent<MeshRenderer>().material = orig;
                orig = null;
                somethingSelected = false;
            }

        }
    }
    void UpdateTravel(Vector3 position, Vector3 forward) {
        if(rightTriggerVal > 0.7f && !isRightTriggerPressed){
            Debug.Log("Right Trigger Val: " + rightTriggerVal);
            isRightTriggerPressed = true;
            RaycastHit hit;
            if(Physics.Raycast(position, forward, out hit,  maxHandRayDist)){
                player.transform.position = hit.point;
            }
            else {
                player.transform.position = position + forward * maxTeleportDist;
                startQuat = Camera.transform.rotation; // gripRotation.action.ReadValue<Quaternion>();

            }
        }
        else if (rightTriggerVal > 0.7f && isRightTriggerPressed){
            player.transform.rotation = gripRotation.action.ReadValue<Quaternion>() ;
            // isRightTriggerPressed = false;
        }
        else if(rightTriggerVal < 0.1f){
            isRightTriggerPressed = false;
        }
    }

    void UpdateSpawn(Vector3 position, Vector3 forward, Vector3 spawnPoint, Vector3 rightPosition, Vector3 rightForward, Vector3 rightEndPoint, Quaternion gripRotationVal) {
        if(objectToSpawn.Equals("Med Bed") && spawnObj) {
            // if (rightGripVal >  0.7f && !isRightGripPressed && !somethingSelected)
            // {
                float maxDist = 5.0f;
                RaycastHit hit;
                if(Physics.Raycast(rightPosition, rightForward, out hit,  maxDist)){
                    Spawn(firstObj, hit.point, gripRotationVal);
                }
                else {
                    Spawn(firstObj, rightPosition + rightForward * maxDist, gripRotationVal);
                }
                
                isRightGripPressed = true;
            //}
            if(rightGripVal < 0.1f) {
                isRightGripPressed = false;
            }
            spawnObj = false;
        }


        if(objectToSpawn.Equals("Chair") && spawnObj) {
            // if (leftGripVal >  0.7f && !isLeftGripPressed && !somethingSelected)
            // {
                float maxDist = 5.0f;
                RaycastHit hit;
                if(Physics.Raycast(rightPosition, rightForward, out hit,  maxDist)){
                    Spawn(secondObj, hit.point, gripRotationVal);

                }
                else {
                    Spawn(secondObj, rightPosition + rightForward * maxDist, gripRotationVal);
                }
                isLeftGripPressed = true;
            //}
            if(leftGripVal < 0.1f) {
                isLeftGripPressed = false;
            }        
            spawnObj = false;
        }
    }
    void Spawn(GameObject preFab, Vector3 position, Quaternion rotationVal) {
        if (preFab.GetComponent<Rigidbody>() == null) {
            preFab.AddComponent<Rigidbody>();
         }
        if (preFab.GetComponent<BoxCollider>() == null) {
            preFab.AddComponent<BoxCollider>();
        }
        if (preFab.GetComponent<MeshRenderer>() == null) {
            preFab.AddComponent<MeshRenderer>();
        }
        preFab.tag = "selectable";
        Instantiate(preFab, position, rotationVal);
    }
    
}
