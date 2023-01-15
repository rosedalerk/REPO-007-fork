using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAtPlayer : MonoBehaviour
{

    private Camera mainCam;

    private float sSpeed = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(mainCam.transform.position.x, transform.position.y, mainCam.transform.position.z);
        transform.LookAt(targetPosition);
        // Vector3 lookDirection = mainCam.transform.position - transform.position;
        // lookDirection.Normalize();
        // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-1 * lookDirection), sSpeed * Time.deltaTime);
        // transform.rotation.x = 0;
        // transform.rotation.z = 0;


        Vector3 oldRotation = transform.rotation.eulerAngles;
        Vector3 newRotation = new Vector3(oldRotation.x,oldRotation.y+180,oldRotation.z);
        transform.rotation = Quaternion.Euler(newRotation);


        // transform.rotation = Quaternion.Slerp(transform.rotation, -1 * Quaternion.Euler(0, mainCam.transform.eulerAngles.y, 0), sSpeed * Time.deltaTime);
    }
}
