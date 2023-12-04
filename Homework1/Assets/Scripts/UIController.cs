using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    public ARController arController;
    //public ARInteractionController arController;
    public List<Button> buttonObjects = new List<Button>();
    //public Scene otherScene;

    void Start()
    {
        // Add listeners for buttons
        buttonObjects[0].onClick.AddListener(() => arController.SelectPrefabToInstantiate(0));
        buttonObjects[1].onClick.AddListener(() => arController.SelectPrefabToInstantiate(1));
        buttonObjects[2].onClick.AddListener(() => arController.SelectPrefabToInstantiate(2));
        buttonObjects[3].onClick.AddListener(() => arController.SelectPrefabToInstantiate(3));
        buttonObjects[4].onClick.AddListener(() => arController.SelectPrefabToInstantiate(4));
    }


    public void LoadOtherScene()
    {
        //SceneManager.LoadScene(otherScene.name);
    }

}
