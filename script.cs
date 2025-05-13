using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using UnityEngine.UI;

using TMPro;


public class PicchuCourseScript : MonoBehaviour
{
    public TextAsset file;
    public GameObject picchu;
    public GameObject player;
    public GameObject camera;
    public GameObject drone;
    public GameObject cockpit;
    public TextMeshProUGUI userTime;
    public Vector3 picchuCenter;
    public List<Vector3> checkpoints;
    public Material checkpointMaterial;
    public Material checkpointCompleteMaterial;
    public XRHandSubsystem xrhand;
    public Quaternion leftRotation;
    public MetaAimHand rightHand;
    public float timer = 3f;
    public bool isTimerShown = false;
    public bool isStopwatchShown = false;
    public float stopwatch = 0f;
    public List<GameObject> Spheres;
    public List<LineRenderer> lines;
    public bool isCollided = false;
    public Vector3 lastCheckpoint;
    public LineRenderer lineRenderer;

    public LineRenderer debugLine;
    public bool tipTouch = false;
    public bool thirdPerson = true;
    public float viewCounter = 0f;
    // Start is called before the first frame update
    void Start()
    {

        isTimerShown = true;
        leftRotation = Quaternion.identity;
        cockpit.SetActive(false);
        if(picchu != null && player != null) {

            xrhand = XRGeneralSettings.Instance?
                .Manager?
                .activeLoader?
                .GetLoadedSubsystem<XRHandSubsystem>();

            // if (xrhand != null) {
            //     xrhand.updatedHands += OnHandUpdate;

            // }
            if(xrhand == null){
                Debug.Log("XRHAND IS NULL");
                return;// exit(1);
            }
        
  
            checkpoints = ParseFile();
            if (checkpoints.Count > 0) {

                drone.transform.position = checkpoints[0];// + new Vector3(0, 10f, 0); 
                player.transform.position = drone.transform.position + new Vector3(0, 3f, 0) + camera.transform.forward * 2f + camera.transform.up * 3f;
                lastCheckpoint = player.transform.position;
                //getting direction of next checkpoint TODO NOT WORKING FULLY
                if (false) {
                    //gets the vector in between the checkpoint and player
                    Vector3 dir = checkpoints[1] - player.transform.position;
                    dir.y = 0f; 
                    //creates rotation and makes sure not to rotate it vertically
                    Quaternion rot = Quaternion.LookRotation(dir);
                    //assigns horizontal to player rotation using only y
                    // player.transform.rotation = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
                    // drone.transform.rotation = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
                    // drone.transform.rotation = rot;
                    // camera.transform.rotation = drone.transform.rotation;
                }
            }

            DisplayCheckPoints();
        }
        lines.Add(null);
        for( int i = 0; i < Spheres.Count - 1; i++){
            lineRenderer = Spheres[i].GetComponent<LineRenderer>();
            if(lineRenderer == null) {
                lineRenderer = Spheres[i].AddComponent<LineRenderer>();
            }
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
            lineRenderer.startWidth = 0.09f;
            lineRenderer.endWidth = 0.09f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Spheres[i].transform.position);
            lineRenderer.SetPosition(1, Spheres[i+1].transform.position);
            lines.Add(lineRenderer);
        }


        lines[1].startColor = Color.blue;
        lines[1].endColor = Color.red;
    
        debugLine = player.GetComponent<LineRenderer>();
        if(debugLine == null) {
            debugLine = player.AddComponent<LineRenderer>();
        }
        debugLine.material = new Material(Shader.Find("Sprites/Default"));
        debugLine.startColor = Color.white;
        debugLine.endColor = Color.white;
        debugLine.startWidth = 0.1f;
        debugLine.endWidth = 0.1f;
        debugLine.positionCount = 2;
        debugLine.SetPosition(0, Spheres[0].transform.position);
        debugLine.SetPosition(1, Spheres[1].transform.position);


        drone.transform.position = player.transform.position;
        

    }
    void OnHandUpdate(XRHandSubsystem subsystem,
                    XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
                    XRHandSubsystem.UpdateType updateType)
    {
        switch (updateType)
        {
            case XRHandSubsystem.UpdateType.Dynamic:
                // Update game logic that uses hand data
                break;
            case XRHandSubsystem.UpdateType.BeforeRender: 
                // Update visual objects that use hand data
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //user Time position setting so it's positioned with the camera
        Vector3 offset = camera.transform.forward * 2f + camera.transform.right * 0.5f - camera.transform.up * 0.25f;        
        userTime.transform.position = camera.transform.position + offset;
        XRHand leftxrhand = xrhand.leftHand;
        XRHand rightxrhand = xrhand.rightHand;

        if(leftxrhand.isTracked && rightxrhand.isTracked )
        {
                XRHandJoint leftIndexTip = leftxrhand.GetJoint(XRHandJointID.IndexTip);
                XRHandJoint leftIndexMetacarpal = leftxrhand.GetJoint(XRHandJointID.IndexMetacarpal);

                XRHandJoint rightIndexTip = rightxrhand.GetJoint(XRHandJointID.IndexTip);
                XRHandJoint rightThumbTip = rightxrhand.GetJoint(XRHandJointID.ThumbTip);
                
                if (leftIndexTip.TryGetPose(out Pose poseIndTipL) && 
                        leftIndexMetacarpal.TryGetPose(out Pose poseIndMetL) && 
                        rightIndexTip.TryGetPose(out Pose poseIndTipR) && 
                        rightThumbTip.TryGetPose(out Pose poseThmTipR))
                {
                    if(Vector3.Distance(poseIndTipR.position, poseThmTipR.position) < 0.01){
                        drone.transform.position += poseIndTipL.forward;
                        debugLine.SetPosition(0, poseIndTipL.position);
                        debugLine.SetPosition(1, poseIndTipL.position + poseIndTipL.forward*5.0f);
                    }
        
                }
        }
        //kind of fixed drone position so it's not totally fucked
        float centerOffset = 0.3f;
        player.transform.position = drone.transform.position + camera.transform.forward * 2f;
        drone.transform.position += drone.transform.up * centerOffset;
        drone.transform.rotation = camera.transform.rotation;
        drone.transform.position -= drone.transform.up * centerOffset;

        

        if(leftxrhand.isTracked && rightxrhand.isTracked ) {
            XRHandJoint leftIndexTip = leftxrhand.GetJoint(XRHandJointID.IndexTip);
            XRHandJoint rightIndexTip = rightxrhand.GetJoint(XRHandJointID.IndexTip);            
            if (leftIndexTip.TryGetPose(out Pose poseIndTipL) && 
                    rightIndexTip.TryGetPose(out Pose poseIndTipR)){
                            if(Vector3.Distance(poseIndTipL.position, poseIndTipR.position) < 0.01 && !tipTouch){
                                viewCounter++;
                                //first person
                                if(viewCounter == 1f) {
                                    thirdPerson = false;
                                    drone.GetComponent<MeshRenderer>().enabled = false; 
                                }
                                //cockpit
                                else if(viewCounter == 2f) {
                                    thirdPerson = false;
                                    drone.SetActive(false);
                                    cockpit.SetActive(true);  
                                    cockpit.transform.SetParent(camera.transform);
                                    cockpit.transform.localPosition = new Vector3(0.0f, -0.45f, 1f);
                                    
                                    cockpit.transform.localRotation = Quaternion.identity;
                                    viewCounter = -1f; 
                                }
                                //3rd person
                                else if(viewCounter == 0f) {
                                    thirdPerson = true; 
                                    drone.GetComponent<MeshRenderer>().enabled = true; 
                                    drone.SetActive(true);
                                    cockpit.SetActive(false); 

                                }
                                tipTouch = true;
                                Debug.Log("tipTouch: " + tipTouch);
                                Debug.Log("viewCounter: " + viewCounter);


                            }
                            if(Vector3.Distance(poseIndTipL.position, poseIndTipR.position) >= 0.01 ){
                                tipTouch = false;
                            }
                    }
        }


        if(isCollided){
            timer = 3f; 
            isTimerShown = true;
            isCollided = false;
        }
        


        if(isTimerShown) {
            if(timer > 0f) {
                timer -= Time.deltaTime;
                userTime.text = timer.ToString();
                drone.transform.position = lastCheckpoint;
                // cockpit.transform.position = lastCheckpoint + new Vector3(0f, 0.4f, 0f);
                //camera.transform.position = lastCheckpoint;
                //player.transform.position = lastCheckpoint;
            }
            else {
                Debug.Log("Timer is done");
                isTimerShown = false;
                isStopwatchShown = true;

            }
        }
        if(isStopwatchShown) {
            stopwatch += Time.deltaTime;
            if(checkpoints.Count == 0) {
                isStopwatchShown = false;
            }
        }
        if(!isTimerShown) {
            userTime.text = stopwatch.ToString();
        }
        if(checkpoints.Count == 0){
            return;
        }
        if(Vector3.Distance(player.transform.position, checkpoints[0]) <= 9.144){
            // Debug.Log("Reached Checkpoint!!");
            Spheres[0].GetComponent<Renderer>().material = checkpointCompleteMaterial;
            lastCheckpoint = checkpoints[0];
            Spheres.RemoveAt(0);
            checkpoints.RemoveAt(0);

            if(lines[0] == null){
                lines.RemoveAt(0);
                return;
            }
            lines[0].startColor = Color.blue;
            lines[0].endColor = Color.blue;
            if (lines.Count == 1) return;
            lines.RemoveAt(0);
            lines[0].startColor = Color.blue;
            lines[0].endColor = Color.red;
        }
        if(thirdPerson){
                player.transform.position = drone.transform.position + drone.transform.up * 2f - drone.transform.forward * 3f;
        }
        else if (viewCounter == -1f) {
        //     cockpit.transform.localPosition = camera.transform.position + new Vector3(0f, -0.5f, -0.1f);
        //     cockpit.transform.rotation = camera.transform.rotation;
            Debug.Log("COCKPIT " + cockpit.transform.position);
            Debug.Log("camera " + camera.transform.position);
        //     Debug.Log("viewCounter " + viewCounter);

        }
        else if (viewCounter == 1f) {
            player.transform.position = drone.transform.position + drone.transform.up * 0.27f + drone.transform.forward * 0.85f;
        }
        drone.transform.Rotate(0, 90, 0);
    }

    List<Vector3> ParseFile()
    {
        float ScaleFactor = 1.0f / 39.37f;
        List<Vector3> positions = new List<Vector3>();
        string content = file.ToString();
        string[] lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] coords = lines[i].Split(' ');
            Vector3 pos = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
            Vector3 worldPos = (pos * ScaleFactor);
            positions.Add(worldPos);
        }
        return positions;
    }
    void DisplayCheckPoints() {
        foreach(Vector3 point in checkpoints) {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.transform.position = point;
            s.GetComponent<Renderer>().material = checkpointMaterial;
            s.GetComponent<Collider>().enabled = false;
            // s.transform.scale = new Vector3(30f, 30f, 30f);
            Spheres.Add(s);
        }
    }
}