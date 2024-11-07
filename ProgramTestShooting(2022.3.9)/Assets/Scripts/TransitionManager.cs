using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeOut(float duration, Action onComplete)
    {
        canvasGroup.alpha = 1;

        LeanTween.alphaCanvas(canvasGroup, 0f, duration)
            .setEase(LeanTweenType.easeInCirc)
            .setOnComplete(() => onComplete?.Invoke());
    }

    public void FadeIn(float duration, Action onComplete)
    {
        canvasGroup.alpha = 0;

        LeanTween.alphaCanvas(canvasGroup, 1f, duration)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() => onComplete?.Invoke());
    }
}
