//using UnityEngine;
//using UnityEngine.UI;

//public class ScoreManager : MonoBehaviour
//{
//    public int score = 0;
//    public Text scoreText; // Gán Text UI ở Inspector

//    public void AddScore(int points)
//    {
//        score += points;
//        UpdateScoreUI();
//    }

//    private void UpdateScoreUI()
//    {
//        if (scoreText != null)
//            scoreText.text = "Score: " + score;
//    }
////}
//using TMPro;
//using UnityEngine;

//public class ScoreManager : MonoBehaviour
//{
//    public int score = 0;
//  //  public TMP_Text scoreText; // Sử dụng TMP_Text – dùng được cả TextMeshPro và TextMeshProUGUI

//    public void AddScore(int points)
//    {
//        score += points;
//       // UpdateScoreUI();
//    }

//    public int GetScore()
//    {
//        return score;
//    }
//    // private void UpdateScoreUI()
//    // {
//    //    if (scoreText != null)
//    //         scoreText.text = "Score: " + score;
//    // }
//}

using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager PlayerScoreManager; // Chỉ dùng cho P1 thôi
    public TMP_Text scoreText; // Sử dụng TMP_Text – dùng được cả TextMeshPro và TextMeshProUGUI
    public int score = 0;

    private void Awake()
    {
        // Gán PlayerScoreManager chỉ nếu đây là player chính
        if (gameObject.name.StartsWith("P1_")) // hoặc gắn tag trước
        {
            PlayerScoreManager = this;
        }
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public int GetScore()
    {
        return score;
    }
    private void Update()
    {
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}
