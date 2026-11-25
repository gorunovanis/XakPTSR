using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float amplitude = 0.5f;
    public float frequency = 0.5f;
    public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

    private Vector3 startPos;
    private float phaseOffset;

    private void Start()
    {
        startPos = transform.position;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency + phaseOffset) * amplitude;
        transform.position = startPos + new Vector3(0f, yOffset, 0f);

        transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
    }
}
