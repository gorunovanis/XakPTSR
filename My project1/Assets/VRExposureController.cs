using UnityEngine;
using UnityEngine.Video;

public class VRExposureController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public GameObject exposureSphere;

    [Header("Intensity")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float eventDensity = 1f;
    [Range(0f, 1f)] public float flashIntensity = 1f;

    [Header("Triggers")]
    public float[] pauseTimecodes;
    private int nextTriggerIndex = 0;

    private bool isPlaying = false;

    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        UpdateAudio();
    }

    public void StartExposure()
    {
        if (!videoPlayer) return;

        exposureSphere.SetActive(true);
        videoPlayer.Play();
        if (audioSource)
        {
            audioSource.Play();
        }

        isPlaying = true;
        nextTriggerIndex = 0;
    }

    public void PauseExposure()
    {
        if (!videoPlayer) return;

        videoPlayer.Pause();
        if (audioSource) audioSource.Pause();
        isPlaying = false;
    }

    public void ResumeExposure()
    {
        if (!videoPlayer) return;

        videoPlayer.Play();
        if (audioSource) audioSource.UnPause();
        isPlaying = true;
    }

    public void StopExposure()
    {
        if (!videoPlayer) return;

        videoPlayer.Stop();
        if (audioSource) audioSource.Stop();
        exposureSphere.SetActive(false);
        isPlaying = false;
    }

    public void HideExposureVisual()
    {
        exposureSphere.SetActive(false);
    }

    public void ShowExposureVisual()
    {
        exposureSphere.SetActive(true);
    }

    private void Update()
    {
        if (!isPlaying || !videoPlayer) return;

        if (pauseTimecodes != null && nextTriggerIndex < pauseTimecodes.Length)
        {
            if (videoPlayer.time >= pauseTimecodes[nextTriggerIndex])
            {
                Debug.Log($"[Exposure] Pause suggestion at {videoPlayer.time:F1}s");
                nextTriggerIndex++;
            }
        }

        UpdateAudio();
    }

    private void UpdateAudio()
    {
        if (audioSource)
        {
            audioSource.volume = volume;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("[Exposure] Video ended");
        SessionManager.Instance.AddEvent("ExposureEnd");
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        UpdateAudio();
        SessionManager.Instance.AddEvent("SetVolume", -1, $"volume={volume}");
    }

    public void SetEventDensity(float d)
    {
        eventDensity = Mathf.Clamp(d, 0.5f, 2f);
        SessionManager.Instance.AddEvent("SetEventDensity", -1, $"density={eventDensity}");
    }

    public void SetFlashIntensity(float f)
    {
        flashIntensity = Mathf.Clamp01(f);
        SessionManager.Instance.AddEvent("SetFlashIntensity", -1, $"flash={flashIntensity}");
    }
}
