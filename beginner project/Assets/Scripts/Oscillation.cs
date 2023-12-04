using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillation : MonoBehaviour
{
    public float speed = 5.0f; // Speed of oscillation
    public float amplitude = 0.5f; // Amplitude of oscillation

    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position of the cube
        startPosition = transform.position;
    }

    void Update()
    {
        // Oscillate the cube's position over time
        transform.position = startPosition + new Vector3(0.0f, Mathf.Sin(Time.time * speed) * amplitude, 0.0f);
    }
}
