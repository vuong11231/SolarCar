//using UnityEngine;
//using UnityEngine.UI;

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

//        private bool _started;


//        private void Start()
//        {
//            currentLapTime = 9999f;
//            bestLapTime = 9999f;
//            previousLapTime = 9999f;
//        }


//        private void Update()
//        {
//            if (!_started)
//            {
//                return;
//            }

//            currentLapTime += Time.deltaTime;

//            if (currentLapTime < 9998f)
//            {
//                currentLapTimeText.text = currentLapTime.ToString("F2");
//            }

//            if (previousLapTime < 9998f)
//            {
//                previousLapTimeText.text = previousLapTime.ToString("F2");
//            }

//            if (bestLapTime < 9998f)
//            {
//                bestLapTimeText.text = bestLapTime.ToString("F2");
//            }
//        }


//        private void OnTriggerEnter(Collider other)
//        {
//            _started = true;

//            if (currentLapTime < 5f)
//            {
//                return;
//            }

//            if (currentLapTime < bestLapTime)
//            {
//                bestLapTime = currentLapTime;
//            }

//            previousLapTime = currentLapTime;
//            currentLapTime = 0f;
//        }
//    }
//}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        public GameObject endGameUI;
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
            if (!other.CompareTag("Car")) return; // Đảm bảo chỉ trigger với xe

            triggerCount++;

            if (triggerCount == 1)
            {
                // Lần đầu tiên -> Bắt đầu tính giờ
                currentLapTime = 0f;
                isTiming = true;
            }
            else if (triggerCount == 2)
            {
                // Lần thứ hai -> Dừng tính giờ, hiện UI
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
            }
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

