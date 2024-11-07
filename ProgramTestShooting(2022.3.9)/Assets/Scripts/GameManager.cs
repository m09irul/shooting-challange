using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SpriteRenderer screenFlash;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameoverTxt;
    public StageLoop stageLoop;
    public bool isGameOver { get; private set; }
    public float leftScreenBound { get; private set; }
    public float rightScreenBound { get; private set; }
    public float topScreenBound { get; private set; }
    public float bottomScreenBound { get; private set; }
    public Image playerHealthFill;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        CalculateScreenBounds();
    }
    public void FlashScreen(Gradient color, float time)
    {
        screenFlash.gameObject.SetActive(true);

        LeanTween.value(gameObject, 0f, 1f, time)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float value) =>
            {
                
                screenFlash.color = color.Evaluate(value);
            })
            .setOnComplete(() =>
            {
                screenFlash.gameObject.SetActive(false);
            });
    }
    public void GameOver()
    { 
        isGameOver = true;
    }
    public void DrawGameOverPanel(int currentStageNumber)
    {
        gameOverPanel.SetActive(true);
        gameoverTxt.text = $"You cleared {currentStageNumber} stage";
    }
    private void CalculateScreenBounds()
    {
        var mainCamera = Camera.main;

        leftScreenBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        rightScreenBound = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        topScreenBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        bottomScreenBound = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
    }

}
