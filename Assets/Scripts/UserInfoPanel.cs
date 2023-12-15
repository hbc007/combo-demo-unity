using Seayoo.OmniSDK;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoPanel : MonoBehaviour
{
    public Text userId;
    public Text signature;

    public void Show(OmniSDKLoginInfo info)
    {
        this.gameObject.SetActive(true);
        userId.text = "User Id: " + info.userId;
        signature.text = "Signature: " + info.signature;
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
