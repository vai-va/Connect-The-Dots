using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

// LevelData class holds data for a single level.
[System.Serializable]
public class LevelData
{
    public string[] level_data;
}

// LevelDataWrapper class holds a list of LevelData objects.
[System.Serializable]
public class LevelDataWrapper
{
    public List<LevelData> levels;
}

public class GameController : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Canvas canvas;    // The game's main canvas.
    private LevelDataWrapper levelDataWrapper;   // Wrapper for level data objects.

    // Button objects that track the first and last buttons clicked.
    private ButtonController firstClickedButton;
    private ButtonController lastClickedButton;

    private int levelIndex;     // Index for the current level.

    private Queue<IEnumerator> lineDrawQueue = new Queue<IEnumerator>(); // Queue to hold all lines drawn during the game.
    private bool isLineBeingDrawn = false;  // Flag to track if a line is currently being drawn.
    public GameObject linePrefab;

    public LevelCompleteScreen LevelCompleteScreen;


    // Property to track the current expected number.
    public int CurrentExpectedNumber { get; private set; } = 1;

    void Start()
    {
        levelIndex = LevelSelector.selectedLevel-1;
        LoadLevelData();
        SetupLevel();
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

    // SetupLevel method sets up a new level based on loaded level data.
    void SetupLevel()
    {
        LevelData levelData = levelDataWrapper.levels[levelIndex];
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float scaleFactor = canvas.scaleFactor;
        float screenWidth = Screen.width / scaleFactor;
        float screenHeight = Screen.height / scaleFactor;

        CurrentExpectedNumber = 1;
        int totalButtons = levelData.level_data.Length / 2;

        // We'll restrict the x and y values to be within 2% of the screen size from the edges
        float marginPercent = 0.05f;

        for (int i = totalButtons - 1; i >= 0; i--)
        {
            // Create a new button object
            GameObject newButton = Instantiate(buttonPrefab, canvas.transform);

            // Set its position
            float x = (float.Parse(levelData.level_data[i * 2]) / 1000 * (1 - 2 * marginPercent) + marginPercent) * screenWidth;
            float y = screenHeight - (float.Parse(levelData.level_data[i * 2 + 1]) / 1000 * (1 - 2 * marginPercent) + marginPercent) * screenHeight; // Y inversion for Unity's UI
            newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, -y); // Negated Y for Unity's UI system

            // Assign button script and set the expected number
            ButtonController buttonController = newButton.GetComponent<ButtonController>();
            buttonController.SetNumber(i + 1);
            buttonController.SetGameController(this);
        }
    }


    // ButtonClickedCorrectly method handles the action when a button is clicked in the correct order.
    public void ButtonClickedCorrectly(ButtonController buttonController)
    {
        if (CurrentExpectedNumber == 1) // After the first button has been clicked and CurrentExpectedNumber incremented
        {
            firstClickedButton = buttonController;
        }

        // increment the expected number when a button is clicked in the correct order
        CurrentExpectedNumber++;

        // If there's a previously clicked button and a line is not currently being drawn,
        // start drawing a line from it to the newly clicked button.
        if (lastClickedButton != null && !isLineBeingDrawn)
        {
            StartCoroutine(DrawLineBetweenButtons(lastClickedButton, buttonController));
        }
        else if (lastClickedButton != null)
        {
            // If a line is currently being drawn, add this line to the queue to be drawn next.
            lineDrawQueue.Enqueue(DrawLineBetweenButtons(lastClickedButton, buttonController));
        }

        // If this was the last button (meaning CurrentExpectedNumber has gone past the total number of buttons), 
        // then enqueue a line to draw from this button back to the first button.
        if (CurrentExpectedNumber > levelDataWrapper.levels[levelIndex].level_data.Length / 2)
        {
            lineDrawQueue.Enqueue(DrawLineBetweenButtons(buttonController, firstClickedButton));
        }

        buttonController.MoveToBack();
        
        lastClickedButton = buttonController;

    }



    private IEnumerator DrawLineBetweenButtons(ButtonController buttonA, ButtonController buttonB)
    {
        // Set the flag to true since a line is being drawn.
        isLineBeingDrawn = true;

        // Instantiate a new Line object from the Line prefab.
        GameObject lineObject = Instantiate(linePrefab);
        lineObject.name = "Line";

        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.SetPosition(0, buttonA.transform.position);

        // Animate the line drawing from start to end.
        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            Vector3 currentEndPosition = Vector3.Lerp(buttonA.transform.position, buttonB.transform.position, t);
            lineRenderer.SetPosition(1, currentEndPosition);

            float distance = Vector3.Distance(buttonA.transform.position, currentEndPosition);
            lineRenderer.material.mainTextureScale = new Vector2(distance, 1);

            yield return null;
        }

        // Ensure the line fully extends to the end point.
        lineRenderer.SetPosition(1, buttonB.transform.position);

        float finalDistance = Vector3.Distance(buttonA.transform.position, buttonB.transform.position);
        lineRenderer.material.mainTextureScale = new Vector2(finalDistance, 1);

        // After the line has been drawn, check if there's another line in the queue to be drawn.
        // If there is, start the Coroutine to draw the next line.
        if (lineDrawQueue.Count > 0)
        {
            StartCoroutine(lineDrawQueue.Dequeue());
        }
        else
        {
            // If there are no more lines to be drawn, set the flag to false.
            isLineBeingDrawn = false;

            // If we just drew the last line (from last to first button), trigger the Level Complete screen
            if (buttonA == lastClickedButton && buttonB == firstClickedButton)
            {
                LevelComplete();
            }
        }
    }


    public void LevelComplete()
    {
        LevelCompleteScreen.gameObject.SetActive(true);
    }
}
