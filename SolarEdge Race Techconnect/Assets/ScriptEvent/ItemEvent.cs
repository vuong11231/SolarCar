
//using UnityEngine;

//public class ItemEvent : MonoBehaviour
//{
//    public int pointValue = 10; // Điểm của item
//    public GameObject fxPrefab; // Prefab của hiệu ứng FX (gán sẵn trong Inspector)

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Car")) // Đảm bảo xe có tag là "Car"
//        {
//            ScoreManager scoreManager = other.GetComponent<ScoreManager>();
//            if (scoreManager != null)
//            {
//                scoreManager.AddScore(pointValue);
//            }

//            // Phát hiệu ứng tại vị trí item
//            if (fxPrefab != null)
//            {
//                GameObject fx = Instantiate(fxPrefab, transform.position, Quaternion.identity);
//                Destroy(fx, 2f); // Hủy hiệu ứng sau 2 giây
//            }

//            // Biến mất (ẩn object)
//            gameObject.SetActive(false);
//        }
//    }
//}
using UnityEngine;

public class ItemEvent : MonoBehaviour
{
    public int pointValue = 10; // Điểm của item
    public GameObject fxPrefab; // Prefab của hiệu ứng FX (gán sẵn trong Inspector)
    public AudioClip soundClip; // Âm thanh khi ăn item (gán trong Inspector)
    public float destroyDelay = 2f; // Thời gian huỷ hiệu ứng và âm thanh

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car")) // Đảm bảo xe có tag là "Car"
        {
            // Cộng điểm
            ScoreManager scoreManager = other.GetComponent<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(pointValue);
            }

            // Phát hiệu ứng FX
            if (fxPrefab != null)
            {
                GameObject fx = Instantiate(fxPrefab, transform.position, Quaternion.identity);
                Destroy(fx, destroyDelay);
            }

            // Phát âm thanh nhỏ
            if (soundClip != null)
            {
                AudioSource.PlayClipAtPoint(soundClip, transform.position);
            }

            // Ẩn object
            gameObject.SetActive(false);
        }
    }
}
