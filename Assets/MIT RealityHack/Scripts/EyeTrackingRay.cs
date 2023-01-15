using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class EyeTrackingRay : MonoBehaviour
{
    [SerializeField] private float rayDistance = 1.0f;

    [SerializeField] private float rayWidth = 0.01f;

    [SerializeField] private LayerMask layersToInclude;

    [SerializeField] private Color rayColorDefaultState = Color.yellow;

    [SerializeField] private Color rayColorHoverState = Color.red;

    private LineRenderer _lineRenderer;

    private List<EyeInteractable> eyeInteractables = new List<EyeInteractable>();
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        SetupRay();
    }

    void SetupRay()
    {
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = rayWidth;
        _lineRenderer.startColor = rayColorDefaultState;
        _lineRenderer.endColor = rayColorDefaultState;
        _lineRenderer.SetPosition(0,transform.position);
        _lineRenderer.SetPosition(1,new Vector3(transform.position.x,transform.position.y,transform.position.z + rayDistance));

    }

    void FixedUpdate()
    {
        RaycastHit hit;
        Vector3 rayCastDirection = transform.TransformDirection(Vector3.forward) * rayDistance;

        if (Physics.Raycast(transform.position,rayCastDirection,out hit,Mathf.Infinity,layersToInclude))
        {
            UnSelect();
            _lineRenderer.startColor = rayColorHoverState;
            _lineRenderer.endColor = rayColorHoverState;
            var eyeInteractable = hit.transform.GetComponent<EyeInteractable>();
            eyeInteractables.Add(eyeInteractable);
            eyeInteractables.Add(eyeInteractable);
            eyeInteractable.isHovered = true;
        }
        else
        {
            _lineRenderer.startColor = rayColorDefaultState;
            _lineRenderer.endColor = rayColorDefaultState;
            UnSelect(true);
        }
    }

    void UnSelect(bool clear = false)
    {
        foreach (var interactable in eyeInteractables)
        {
            interactable.isHovered = false;
        }

        if (clear)
        {
            eyeInteractables.Clear();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
