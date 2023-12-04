using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchRotate : MonoBehaviour
{
    public float rotationSpeed = 360f; // Degrees per second
    private float totalRotation = 0f;
    private bool isRotating = false;

    void Update()
    {
        if (isRotating)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;
            if (totalRotation + rotationThisFrame > 1080f) // 3 full rotations (3*360)
            {
                rotationThisFrame = 1080f - totalRotation;
                isRotating = false; // Stop rotating after 3 full rotations
            }

            transform.Rotate(0, 0, rotationThisFrame); // Rotate around the Z axis
            totalRotation += rotationThisFrame;
        }
    }

    void OnMouseDown()
    {
        if (!isRotating)
        {
            isRotating = true;
            totalRotation = 0f;
        }
    }
}
