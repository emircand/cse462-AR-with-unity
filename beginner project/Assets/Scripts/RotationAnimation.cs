using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float rotationSpeed = 45.0f; // Rotation speed in degrees per second

    void Update()
    {
        // Rotate the cube around its Y axis at a constant speed
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
