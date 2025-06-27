using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float floatAmplitude = 0.5f;     // Biên độ dao động (lên xuống)
    public float floatFrequency = 1f;       // Tần số dao động

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;       // Tốc độ xoay quanh trục Y

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Dao động lên xuống theo sóng sin
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Xoay quanh trục Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
