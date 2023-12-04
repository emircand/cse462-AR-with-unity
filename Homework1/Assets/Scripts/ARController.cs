using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class ARController : MonoBehaviour
{
    [SerializeField]
    private ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    [SerializeField]
    private GameObject[] spawnablePrefabs;
    private Camera arCam;
    private GameObject selectedObject;
    private int selectedPrefabIndex = 0;
    private bool isObjectMoveable = true; // Flag to track if the selected object is moveable

    void Start()
    {
        selectedObject = null;
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        Vector2 touchPosition = touch.position;

        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (m_RaycastManager.Raycast(touchPosition, m_Hits))
        {
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCam.ScreenPointToRay(touchPosition);
                RaycastHit hitObject;

                if (Physics.Raycast(ray, out hitObject) && hitObject.collider.CompareTag("Moveable"))
                {
                    selectedObject = hitObject.transform.gameObject;
                    isObjectMoveable = true; // The object is moveable
                }
                else if (selectedObject == null)
                {
                    SpawnPrefab(m_Hits[0].pose.position);
                    // Check if the prefab is moveable or not
                    isObjectMoveable = selectedPrefabIndex != 2; // If index is 2, object is not moveable
                }
            }
            else if (touch.phase == TouchPhase.Moved && selectedObject != null && isObjectMoveable)
            {
                selectedObject.transform.position = m_Hits[0].pose.position;
            }
            else if (touch.phase == TouchPhase.Ended && selectedObject != null)
            {
                selectedObject = null;
            }
        }
    }

    private void SpawnPrefab(Vector3 spawnPosition)
    {
        selectedObject = Instantiate(spawnablePrefabs[selectedPrefabIndex], spawnPosition, Quaternion.identity);
    }

    public void SelectPrefabToInstantiate(int prefabIndex)
    {
        if(prefabIndex >= 0 && prefabIndex < spawnablePrefabs.Length)
        {
            selectedPrefabIndex = prefabIndex;
        }
        
    }
}