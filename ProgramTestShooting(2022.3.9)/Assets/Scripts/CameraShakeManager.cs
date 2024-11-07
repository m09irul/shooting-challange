using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance { get; private set; }

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noisePerlin;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            noisePerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            float intensity = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
            noisePerlin.m_AmplitudeGain = intensity;

            if (shakeTimer <= 0f)
            {
                noisePerlin.m_AmplitudeGain = 0f;
            }
        }
    }

    public void ShakeCamera(float intensity = -1f, float time = -1f)
    {
        if (noisePerlin != null)
        {
            float shakeIntensity = intensity < 0 ? 0 : intensity;
            float shakeTime = time < 0 ? 0 : time;

            if (noisePerlin.m_AmplitudeGain < shakeIntensity)
            {
                noisePerlin.m_AmplitudeGain = shakeIntensity;
                shakeTimer = shakeTime;
                shakeTimerTotal = shakeTime;
                startingIntensity = shakeIntensity;
            }
        }
    }
}
