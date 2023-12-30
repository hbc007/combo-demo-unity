using Combo;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoPanel : MonoBehaviour
{
    public Text loginInfo;

    public void Show(ComboSDKLoginInfo info)
    {
        this.gameObject.SetActive(true);
        loginInfo.text = $"ComboId:\n {info.comboId}\n";
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
