using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUpdater : MonoBehaviour
{
    public TextMesh textObject; // Assign this in the inspector
    public string message = "Hello, World!"; // Default message

    void Start()
    {
        if (textObject != null)
        {
            // Update the text of the UI Text component
            textObject.text = message;
        }
    }
}
