using UnityEngine;

public class OscillateMovement : MonoBehaviour
{
    public Vector3 direction = Vector3.right; // Hướng dao động (ví dụ: Vector3.right = X)
    public float distance = 1f;               // Khoảng cách dao động tối đa
    public float speed = 1f;                  // Tốc độ dao động

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Tính toán vị trí dao động bằng hàm sin
        float offset = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + direction.normalized * offset;
    }
}
