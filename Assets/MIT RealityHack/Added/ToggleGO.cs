using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGO : MonoBehaviour
{

    public GameObject container;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle() {
        container.SetActive (!container.activeInHierarchy);
    }
}
