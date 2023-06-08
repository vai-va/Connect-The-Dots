using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelector : MonoBehaviour
{
    public static int selectedLevel;
    public TextMeshProUGUI levelText;


    public void OpenScene()
    {
        selectedLevel = int.Parse(levelText.text);
        SceneManager.LoadScene("Level");
    }
}
