using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompleteScreen : MonoBehaviour
{

    public void Setup()
    {
        gameObject.SetActive(true);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene("Level");
    }

    public void GoToLevelSelectButton()
    {
        SceneManager.LoadScene("LevelMenu");
    }

}
