using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameToMenu : MonoBehaviour
{
    public KeyCode mainMenuKey;

    private void Update()
    {
        if (Input.GetKeyDown(mainMenuKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;


            sceneManager.Instance.LoadMainMenu();
        }
    }
}
