using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    private Vector3 _offset;

    // Start is called before the first frame update
    void Start()
    {
        _offset = transform.position - Target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Target.position + _offset;
    }
}