using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EyeInteractable : MonoBehaviour
{
    public bool isHovered { get; set; }
    //public bool isGaze;
    [SerializeField] private UnityEvent<GameObject> OnObjectHover;

    [SerializeField] private Material OnHoverActiveMaterial;

    [SerializeField] private Material OnHoverInActiveMaterial;

    private MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start() => meshRenderer = GetComponent<MeshRenderer>();

    void Update()
    {
        if (isHovered)
        {
            //meshRenderer.material = OnHoverActiveMaterial;
            OnObjectHover?.Invoke(gameObject);
        }
        else
        {
            //meshRenderer.material = OnHoverActiveMaterial;
        }
    }
    // Update is called once per frame
}