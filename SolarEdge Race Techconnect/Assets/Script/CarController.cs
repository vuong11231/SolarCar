using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public float moveSpeed = 10f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Lấy đầu vào từ bàn phím
        float horizontal = Input.GetKey(KeyCode.D) ? 1 :
                           Input.GetKey(KeyCode.A) ? -1 : 0;

        float vertical = Input.GetKey(KeyCode.W) ? 1 :
                         Input.GetKey(KeyCode.S) ? -1 : 0;

        // Vector di chuyển (8 hướng)
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        // Di chuyển xe
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Xoay xe theo hướng di chuyển (nếu có input)
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 0.15f);
        }
    }
}
