using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldContainer : MonoBehaviour
{

    public GameObject controlLocus;
    public float offset;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(controlLocus.transform.position.x, controlLocus.transform.position.y - offset, controlLocus.transform.position.z);
    }
}
