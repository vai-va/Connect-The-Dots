using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

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
    public TextMeshProUGUI buttonTextPrefab;
    public Canvas canvas;
    private LevelDataWrapper levelDataWrapper;

    void Start()
    {
        LoadLevelData();
        SetupLevel(2); // To set up the first level, for instance
    }

    void LoadLevelData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "level_data.json");

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
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
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < levelData.level_data.Length; i += 2)
        {
            // Create a new button object
            GameObject newButton = Instantiate(buttonPrefab, canvas.transform);

            // Set its position
            float x = float.Parse(levelData.level_data[i]) / 1000 * canvasWidth;
            float y = 1 * float.Parse(levelData.level_data[i + 1]) / 1000 * canvasHeight - canvasHeight; // Y inversion for Unity's UI
            newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

            // Set the button number
            TextMeshProUGUI buttonText = Instantiate(buttonTextPrefab, newButton.transform);
            buttonText.text = (i / 2 + 1).ToString();
        }
    }

}
