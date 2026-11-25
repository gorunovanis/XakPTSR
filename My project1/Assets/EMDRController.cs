using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class EMDRController : MonoBehaviour
{
    [Header("Visual")]
    public Transform visualTarget;
    public Transform leftPoint;
    public Transform rightPoint;
    public AnimationCurve movementCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Audio")]
    public AudioSource leftAudio;
    public AudioSource rightAudio;

    [Header("Params")]
    [Range(1f, 4f)] public float frequencyHz = 2f;
    public bool isRunning = false;

    [Header("Haptics")]
    public bool enableHaptics = true;
    [Range(0f, 1f)] public float hapticAmplitude = 0.6f;
    [Range(0.01f, 0.5f)] public float hapticDuration = 0.08f;
    public XRController leftController;
    public XRController rightController;

    private Coroutine visualRoutine;
    private Coroutine audioRoutine;
    private Coroutine hapticRoutine;

    public void StartEMDR()
    {
        isRunning = true;

        if (visualRoutine != null) StopCoroutine(visualRoutine);
        if (audioRoutine != null) StopCoroutine(audioRoutine);
        if (hapticRoutine != null) StopCoroutine(hapticRoutine);

        visualRoutine = StartCoroutine(VisualEMDR());
        audioRoutine = StartCoroutine(AudioEMDR());

        if (enableHaptics && (leftController != null || rightController != null))
            hapticRoutine = StartCoroutine(HapticEMDR());
    }

    public void StopEMDR()
    {
        isRunning = false;
        if (visualRoutine != null) StopCoroutine(visualRoutine);
        if (audioRoutine != null) StopCoroutine(audioRoutine);
        if (hapticRoutine != null) StopCoroutine(hapticRoutine);

        if (visualTarget)
            visualTarget.gameObject.SetActive(false);

        if (leftAudio) leftAudio.Stop();
        if (rightAudio) rightAudio.Stop();
    }

    private IEnumerator VisualEMDR()
    {
        if (!visualTarget || !leftPoint || !rightPoint)
            yield break;

        visualTarget.gameObject.SetActive(true);

        float period = 1f / frequencyHz;
        bool directionLeftToRight = true;

        while (isRunning)
        {
            Transform from = directionLeftToRight ? leftPoint : rightPoint;
            Transform to = directionLeftToRight ? rightPoint : leftPoint;

            float t = 0f;
            while (t < 1f && isRunning)
            {
                t += Time.deltaTime / period;
                float c = movementCurve.Evaluate(Mathf.Clamp01(t));
                visualTarget.position = Vector3.Lerp(from.position, to.position, c);
                yield return null;
            }

            directionLeftToRight = !directionLeftToRight;
        }
    }

    private IEnumerator AudioEMDR()
    {
        float period = 1f / frequencyHz;

        while (isRunning)
        {
            if (leftAudio)
            {
                leftAudio.Play();
            }
            yield return new WaitForSeconds(period / 2f);

            if (rightAudio)
            {
                rightAudio.Play();
            }
            yield return new WaitForSeconds(period / 2f);
        }
    }

    private IEnumerator HapticEMDR()
    {
        float period = 1f / frequencyHz;

        while (isRunning)
        {
            if (leftController != null)
                leftController.SendHapticImpulse(hapticAmplitude, hapticDuration);

            yield return new WaitForSeconds(period / 2f);

            if (rightController != null)
                rightController.SendHapticImpulse(hapticAmplitude, hapticDuration);

            yield return new WaitForSeconds(period / 2f);
        }
    }

    public void SetFrequency(float hz)
    {
        frequencyHz = Mathf.Clamp(hz, 1f, 4f);
        SessionManager.Instance.AddEvent("SetEMDRFrequency", -1, $"Hz={frequencyHz}");
    }

    public void SetVolume(float v)
    {
        if (leftAudio) leftAudio.volume = v;
        if (rightAudio) rightAudio.volume = v;
        SessionManager.Instance.AddEvent("SetEMDRVolume", -1, $"volume={v}");
    }
}
