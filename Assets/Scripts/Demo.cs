using Combo;
using UnityEngine;
using EasyUI.Toast;
using System.IO;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    public static bool isInit = false;
    public UserInfoPanel userInfoPanel;
    public PurchasePanel purchasePanel;
    public DownloadUrlPanel downloadUrlPanel;
    public Text metaInfo;

    void Start() {
        metaInfo.text =
            $@"DeviceId:{ComboSDK.GetDeviceId()}
ComboSDK v{ComboSDK.GetVersion()} with NativeSDK v{ComboSDK.GetVersionNative()} GameId:{ComboSDK.GetGameId()} - Distro:{ComboSDK.GetDistro()}";
#if UNITY_ANDROID
        metaText.text += $" - Variant:{ComboSDK.GetVariant()} - Subvariant:{ComboSDK.GetSubvariant()}";
#endif
        ComboSDK.OnKickOut(r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                Toast.Show($"踢出成功");
                Debug.Log($"踢出成功, shouldExit: {result.shouldExit}");
                if (result.shouldExit)
                {
                    Application.Quit();
                }
                else
                {
                    Toast.Show($"您已被强制下线");
                    Debug.LogError("用户强制下线");
                }
            }
            else
            {
                var err = r.Error;
                Toast.Show($"踢出失败：{err.Message}");
                Debug.LogError("踢出失败: " + err.DetailMessage);
            }
        });
    }

    public void OnLogin()
    {
        Debug.LogWarning("OnLogin called");

        ComboSDK.Login(r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                Debug.LogWarning($"登录成功: COMBOID - {result.loginInfo.comboId}; TOKEN - {result.loginInfo.identityToken}");
            }
            else
            {
                var error = r.Error;
                if (error.Code == ErrorCode.UserCancelled)
                {
                    Toast.Show("用户取消登录");
                    return;
                }
                Toast.Show($"登录失败：{error.Message}");
                Debug.LogError("登录失败: " + error.DetailMessage);
            }
        });
    }

    public void OnGetUserInfo()
    {
        var info = ComboSDK.GetLoginInfo();
        if (info == null || string.IsNullOrEmpty(info.comboId))
        {
            Toast.Show("用户未登录");
            return;
        }
        userInfoPanel.Show(info);
        Debug.Log($"GetUserInfo: comboId = {info.comboId}," + $"identityToken = {info.identityToken}");
    }

    public void OnLogout()
    {
        ComboSDK.Logout(r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                if (result == null || string.IsNullOrEmpty(result.comboId))
                {
                    Toast.Show($"用户未登录");
                    return;
                }
                Toast.Show($"用户 {result.comboId} 登出成功");
                Debug.Log($"登出成功: UserId - {result.comboId}");
            }
            else
            {
                var err = r.Error;
                Debug.LogError("登出失败: " + err.DetailMessage);
            }
        });
    }

    public void OnPurchase()
    {
        purchasePanel.Show();
    }

    public void OnPreloadAd()
    {
        Debug.LogWarning("OnPreloadAd called");

        var opts = new PreloadAdOptions
        {
            placementId = "test_placement_id",
        };
        Debug.LogWarning("PreloadAd PlacementId: " + opts.placementId);
        ComboSDK.PreloadAd(opts, r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                Toast.Show($"广告 {result.placementId} 预加载成功");
                Debug.Log("广告预加载成功: PlacementId - " + result.placementId);
            }
            else
            {
                var err = r.Error;
                Toast.Show($"广告 {opts.placementId} 预加载失败\n{err.Message}");
                Debug.LogError("广告预加载失败: " + err.DetailMessage);
            }
        });
    }

    public void OnShowAd()
    {
        var opts = new ShowAdOptions
        {
            placementId = "ios_topon_test_01",
        };
        ComboSDK.ShowAd(opts, r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                Toast.Show($"广告 {result.token} 显示成功");
                Debug.Log($"广告显示成功: Status - {result.status}; Token - {result.token}");
            }
            else
            {
                var err = r.Error;
                Toast.Show($"广告 {opts.placementId} 显示失败\n{err.Message}");
                Debug.LogError("广告显示失败: " + err.DetailMessage);
            }
        });
    }
    public void OnForceCrash()
    {
        DemoUtils.ForceCrash();
    }

    public void OnShareLink()
    {
        var opts = new SystemShareOptions
        {
           text = "链接分享",
           linkUrl = "https://docs.seayoo.com/",
        };

        SocialShare(opts);
    }

    public void OnShareLocalImage()
    {
        byte[] bytes = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
        string path = Path.Combine(Application.temporaryCachePath, "sharePicture.png");
        File.WriteAllBytes(path, bytes);

        var opts = new SystemShareOptions
        {
           imageUrl = path
        };

        SocialShare(opts);
    }

    public void OnShareOnlineImage()
    {
        var opts = new SystemShareOptions
        {
           text = "网络图片分享",
           imageUrl = "https://cn.bing.com/th?id=OHR.CERNCenter_EN-US9854867489_1920x1080.jpg"
        };

        SocialShare(opts);
    }

    public void OnGetDownloadUrl() 
    {
        ComboSDK.GetDownloadUrl(r=>{
            if(r.IsSuccess){
                var result = r.Data;
                downloadUrlPanel.Show(result.downloadUrl);
                Toast.Show("获取 Url 成功");
                Debug.Log("获取 Url 成功");
            }
            else{
                var error = r.Error;
                Toast.Show("无法获取 Url: " + error.Message);
                Debug.LogError("获取 Url 失败: " + error.Message);
            }
        });
    }

    public void OnQuit() 
    {
        if (ComboSDK.IsFeatureAvailable(Feature.QUIT))
        {
            Debug.Log("Game Exiting");
            ComboSDK.Quit(result =>
            {
                if (result.IsSuccess)
                {
                    Application.Quit();
                }
                else
                {
                    Debug.LogError("退出失败：" + result.Error);
                }
            });
        }
        else
        {
            Application.Quit();
        }
    }

    private void SocialShare(SystemShareOptions opts) 
    {
        ComboSDK.SocialShare(opts, r =>
        {
           if (r.IsSuccess)
           {
               var result = r.Data;
               Toast.Show("分享成功");
               Debug.Log("分享成功");
           }
           else
           {
               var err = r.Error;
               Toast.Show("分享失败：" + err.Message);
               Debug.LogError("分享失败: " + err.DetailMessage);
           }
        });
    }
}
