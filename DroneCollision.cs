using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCollision : MonoBehaviour
{
    public GameObject picchu;
    public GameObject picchuScript;
    public PicchuCourseScript picchuCourseScript;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == picchu) {
            picchuCourseScript.isCollided = true;
            Debug.Log("Drone collided with Picchu");
            Debug.Log(picchuCourseScript.isCollided);

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if(picchuScript != null) {
            picchuCourseScript = picchuScript.GetComponent<PicchuCourseScript>();
            Debug.Log(picchuCourseScript.isCollided);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
