using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class LevelData
{
    public string[] level_data;
}

[System.Serializable]
public class LevelDataWrapper
{
    public List<LevelData> levels;
}

public class GameController : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Canvas canvas;
    private LevelDataWrapper levelDataWrapper;

    // public accessor for other scripts to know the current expected number
    public int CurrentExpectedNumber { get; private set; } = 1;

    void Start()
    {
        LoadLevelData();
        SetupLevel(2); // To set up the first level, for instance
    }

    void LoadLevelData()
    {
        TextAsset levelDataJson = Resources.Load<TextAsset>("level_data");

        if (levelDataJson != null)
        {
            string jsonData = levelDataJson.text;
            levelDataWrapper = JsonUtility.FromJson<LevelDataWrapper>(jsonData);
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
    }

    void SetupLevel(int levelIndex)
    {
        LevelData levelData = levelDataWrapper.levels[levelIndex];
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float scaleFactor = canvas.scaleFactor;
        float screenWidth = Screen.width / scaleFactor;
        float screenHeight = Screen.height / scaleFactor;

        CurrentExpectedNumber = 1;

        for (int i = 0; i < levelData.level_data.Length; i += 2)
        {
            // Create a new button object
            GameObject newButton = Instantiate(buttonPrefab, canvas.transform);

            // Set its position
            float x = float.Parse(levelData.level_data[i]) / 1000 * screenWidth;
            float y = screenHeight - float.Parse(levelData.level_data[i + 1]) / 1000 * screenHeight; // Y inversion for Unity's UI
            newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -y); // Negated Y for Unity's UI system

            // Assign button script and set the expected number
            ButtonController buttonController = newButton.GetComponent<ButtonController>();
            buttonController.SetNumber(i / 2 + 1);
            buttonController.SetGameController(this);
        }
    }

    public void ButtonClickedCorrectly()
    {
        // increment the expected number when a button is clicked in the correct order
        CurrentExpectedNumber++;
    }
}

