using UnityEngine;

public class UICloseButton : MonoBehaviour
{
    // Kéo thả UI (panel hoặc object chứa UI cần ẩn) vào đây
    public GameObject uiToClose;

    // Hàm gọi khi nhấn nút Close
    public void CloseUI()
    {
        if (uiToClose != null)
        {
            uiToClose.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Chưa gán UI cần đóng vào script.");
        }
    }
}
