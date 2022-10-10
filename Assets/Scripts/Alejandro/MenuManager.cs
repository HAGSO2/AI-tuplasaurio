using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("AlejandroMaze");
    }

    public void QuitGame()
    {
        Debug.Log("Quit.");
        Application.Quit();
    }
}