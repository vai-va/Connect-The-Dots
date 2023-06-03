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

    private ButtonController firstClickedButton;
    private ButtonController lastClickedButton;
    private int levelIndex = 3;     // specify which level to set up

    private Queue<IEnumerator> lineDrawQueue = new Queue<IEnumerator>();
    private bool isLineBeingDrawn = false;
    public GameObject linePrefab;


    // public accessor for other scripts to know the current expected number
    public int CurrentExpectedNumber { get; private set; } = 1;

    void Start()
    {
        LoadLevelData();
        SetupLevel(); // To set up the first level, for instance
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
        float marginPercent = 0.02f;

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

        // Remember this button as the last clicked button.
        lastClickedButton = buttonController;
    }



    private IEnumerator DrawLineBetweenButtons(ButtonController buttonA, ButtonController buttonB)
    {
        // Set the flag to true since a line is being drawn.
        isLineBeingDrawn = true;

        // Instantiate a new Line object from the Line prefab.
        GameObject lineObject = Instantiate(linePrefab);
        lineObject.name = "Line"; // Name the instantiated object

        // Fetch lineRenderer from the instantiated Line object.
        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

        // Ensure the texture mode is set to Tile
        lineRenderer.textureMode = LineTextureMode.Tile;

        // Set initial position of lineRenderer
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
        }
    }



}
