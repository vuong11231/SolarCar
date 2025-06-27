using UnityEngine;

public class HideUIOnClick : MonoBehaviour
{
    public GameObject uiToHide; // UI cần ẩn, ví dụ: panel, canvas con, text...

    public void HideUI()
    {
        if (uiToHide != null)
        {
            uiToHide.SetActive(false);
        }
    }
}
