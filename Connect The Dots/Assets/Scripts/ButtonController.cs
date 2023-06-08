using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButtonController : MonoBehaviour
{
    public Button button;
    public Image buttonImage;
    public TextMeshProUGUI buttonText;
    public Sprite originalButtonSprite;
    public Sprite clickedButtonSprite;
    public float fadeDuration = 1f; // Duration for fading the text.

    private int number;
    private GameController gameController;


    public void SetNumber(int number)
    {
        this.number = number;
        buttonText.text = number.ToString();
    }

    public void SetGameController(GameController gameController)
    {
        this.gameController = gameController;
    }

    void Start()
    {
        button.onClick.AddListener(HandleClick);
        buttonImage.sprite = originalButtonSprite;
        buttonText.raycastTarget = false;
    }

    void HandleClick()
    {
        if (number == gameController.CurrentExpectedNumber)
        {
            buttonImage.sprite = clickedButtonSprite;
            StartCoroutine(FadeText());

            gameController.ButtonClickedCorrectly(this);
        }
    }

    IEnumerator FadeText()
    {
        for (float t = 0.01f; t < fadeDuration; t += Time.deltaTime)
        {
            buttonText.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, t / fadeDuration));
            yield return null;
        }
    }


    public void MoveToBack()
    {
        // Move the button to the beginning of the hierarchy so it will render behind other buttons
        transform.SetSiblingIndex(0);
    }
}
