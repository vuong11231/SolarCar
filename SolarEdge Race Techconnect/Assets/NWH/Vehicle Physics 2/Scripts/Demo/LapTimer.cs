
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;

//namespace NWH.VehiclePhysics2.Demo
//{
//    public class LapTimer : MonoBehaviour
//    {
//        public float bestLapTime = 9999f;
//        public Text bestLapTimeText;

//        public float currentLapTime = 9999f;
//        public Text currentLapTimeText;
//        public float previousLapTime = 9999f;
//        public Text previousLapTimeText;

//        public GameObject endGameUI;
//        public Button resetButton;
//        public Button quitButton;

//        private int triggerCount = 0;
//        private bool isTiming = false;

//        public Text finalLapTimeText;
//        public float finalLapTime = 0f;

//        private void Start()
//        {
//            currentLapTime = 9999f;
//            bestLapTime = 9999f;
//            previousLapTime = 9999f;

//            endGameUI.SetActive(false);

//            resetButton.onClick.AddListener(ResetGame);
//            quitButton.onClick.AddListener(QuitGame);
//        }

//        private void Update()
//        {
//            if (isTiming)
//            {
//                currentLapTime += Time.deltaTime;

//                if (currentLapTime < 9998f)
//                    currentLapTimeText.text = currentLapTime.ToString("F2");
//            }

//            if (previousLapTime < 9998f)
//                previousLapTimeText.text = previousLapTime.ToString("F2");

//            if (bestLapTime < 9998f)
//                bestLapTimeText.text = bestLapTime.ToString("F2");
//        }

//        private void OnTriggerEnter(Collider other)
//        {
//            if (!other.CompareTag("Car")) return; // Đảm bảo chỉ trigger với xe

//            triggerCount++;

//            if (triggerCount == 1)
//            {
//                // Lần đầu tiên -> Bắt đầu tính giờ
//                currentLapTime = 0f;
//                isTiming = true;
//            }
//            else if (triggerCount == 2)
//            {
//                // Lần thứ hai -> Dừng tính giờ, hiện UI
//                isTiming = false;

//                finalLapTime = currentLapTime;

//                if (currentLapTime >= 5f)
//                {
//                    previousLapTime = currentLapTime;
//                    if (currentLapTime < bestLapTime)
//                    {
//                        bestLapTime = currentLapTime;
//                    }
//                }

//                ShowEndGameUI();
//            }
//        }

//        private void ShowEndGameUI()
//        {
//            if (endGameUI != null)
//            {
//                endGameUI.SetActive(true);

//                if (finalLapTimeText != null)
//                {
//                    finalLapTimeText.text = finalLapTime.ToString("F2") + "s";
//                }
//            }
//        }
//        private void ResetGame()
//        {
//            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//        }

//        private void QuitGame()
//        {
//#if UNITY_EDITOR
//            UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//        }
//    }
//}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NWH.VehiclePhysics2.Demo
{
    public class LapTimer : MonoBehaviour
    {
        public float bestLapTime = 9999f;
        public Text bestLapTimeText;

        public float currentLapTime = 9999f;
        public Text currentLapTimeText;
        public float previousLapTime = 9999f;
        public Text previousLapTimeText;

        public GameObject endGameUI;      // UI đầu tiên
        public GameObject endGameUI2;     // UI thứ hai

        public Button resetButton;
        public Button quitButton;

        private int triggerCount = 0;
        private bool isTiming = false;

        public Text finalLapTimeText;
        public float finalLapTime = 0f;

        private void Start()
        {
            currentLapTime = 9999f;
            bestLapTime = 9999f;
            previousLapTime = 9999f;

            endGameUI.SetActive(false);
            if (endGameUI2 != null)
                endGameUI2.SetActive(false);

            resetButton.onClick.AddListener(ResetGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void Update()
        {
            if (isTiming)
            {
                currentLapTime += Time.deltaTime;

                if (currentLapTime < 9998f)
                    currentLapTimeText.text = currentLapTime.ToString("F2");
            }

            if (previousLapTime < 9998f)
                previousLapTimeText.text = previousLapTime.ToString("F2");

            if (bestLapTime < 9998f)
                bestLapTimeText.text = bestLapTime.ToString("F2");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Car")) return;

            triggerCount++;

            if (triggerCount == 1)
            {
                currentLapTime = 0f;
                isTiming = true;
            }
            else if (triggerCount == 2)
            {
                isTiming = false;
                finalLapTime = currentLapTime;

                if (currentLapTime >= 5f)
                {
                    previousLapTime = currentLapTime;
                    if (currentLapTime < bestLapTime)
                    {
                        bestLapTime = currentLapTime;
                    }
                }

                ShowEndGameUI();
            }
        }

        private void ShowEndGameUI()
        {
            if (endGameUI != null)
            {
                endGameUI.SetActive(true);

                if (finalLapTimeText != null)
                {
                    finalLapTimeText.text = finalLapTime.ToString("F2") + "s";
                }

                // Bắt đầu coroutine để chuyển từ UI1 sang UI2 sau 3 giây
                StartCoroutine(SwitchToSecondUIAfterDelay(3f));
            }
        }

        private IEnumerator SwitchToSecondUIAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (endGameUI != null)
                endGameUI.SetActive(false);

            if (endGameUI2 != null)
                endGameUI2.SetActive(true);
        }

        private void ResetGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

