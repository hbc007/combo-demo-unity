using Seayoo.OmniSDK;
using UnityEngine;
using EasyUI.Toast;
using System.IO;
using System.Collections.Generic;
using System.Xml.Schema;

public class Demo : MonoBehaviour
{
    public static bool isInit = false;
    public UserInfoPanel userInfoPanel;
    public PurchasePanel purchasePanel;
    public TrackPanel trackPanel;
    void Start()
    {
        OmniSDK.Setup(r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Debug.LogWarning($"Initialization Successful: APPID - {result.appId}; CHANNELNAME - {result.channelName}; PLANID - {result.planId}; SDKVERSION - {result.sdkVersion}; CPSNAME - {result.cpsName}");
            }
            else
            {
                var err = r.GetError();
                Toast.Show($"初始化失败：{err.Message}");
                Debug.LogError("初始化失败: " + err.DetailMessage);
            }
        });
    }

    public void OnLogin()
    {
        Debug.LogWarning("OnLogin called");

        OmniSDK.Login(r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Debug.Log("登录成功: " + result.loginInfo.userId);
            }
            else
            {
                var err = r.GetError();
                if (err.Code == OmniSDKErrorCode.UserCancelled)
                {
                    Toast.Show("用户取消登录");
                }
                else 
                {
                    Debug.LogError("登录失败: " + err.DetailMessage);
                    Toast.Show($"登录失败：{err.Message}");
                }
            }
        });
    }

    public void OnGetUserInfo()
    {
        var info = OmniSDK.GetLoginInfo();
        if (info == null)
        {
            Toast.Show("请先登录");
            return;
        }
        userInfoPanel.Show(info);
        Debug.Log($"GetUserInfo: userId = {info.userId}," +
        $"signature = {info.signature} authTime = {info.authTime}");
    }

    public void OnLogout()
    {
        Debug.LogWarning("OnLogout called");
        OmniSDK.Logout(r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Toast.Show($"用户 {result.userId} 登出成功");
                Debug.Log($"登出成功: UserId - {result.userId}");
            }
            else
            {
                var err = r.GetError();
                Debug.LogError("登出失败: " + err.DetailMessage);
            }
        });
    }

    public void OnPurchase()
    {
        purchasePanel.Show();
    }

    public void OnTrack()
    {
        trackPanel.Show();
    }

    public void OnPreloadAd()
    {
        Debug.LogWarning("OnPreloadAd called");

        var opts = new OmniSDKAdOptions
        {
#if UNITY_ANDROID
            placementId = "android_launch_sense_award_1",
#elif UNITY_IOS
            placementId = "test_ios_RV_1",
#endif
        };
        Debug.LogWarning("PreloadAd PlacementId: " + opts.placementId);
        OmniSDK.PreloadAd(opts, r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Toast.Show($"广告 {result.placementId} 预加载成功", 2f, ToastColor.Black);
                Debug.Log("广告预加载成功: PlacementId - " + result.placementId);
            }
            else
            {
                var err = r.GetError();
                Toast.Show($"广告 {opts.placementId} 预加载失败\n{err.Message}");
                Debug.LogError("广告预加载失败: " + err.DetailMessage);
            }
        });
    }

    public void OnShowAd()
    {
        Debug.LogWarning("OnShowAd called");

        var opts = new OmniSDKAdOptions
        {
#if UNITY_ANDROID
            placementId = "android_launch_sense_award_1",
#elif UNITY_IOS
            placementId = "test_ios_RV_1",
#endif
        };
        Debug.LogWarning("ShowAd PlacementId: " + opts.placementId);
        OmniSDK.ShowAd(opts, r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Toast.Show($"广告 {result.token} 显示成功", 2f, ToastColor.Black);
                Debug.Log($"广告显示成功: Status - {result.status}; Token - {result.token}");
            }
            else
            {
                var err = r.GetError();
                Toast.Show($"广告 {opts.placementId} 显示失败\n{err.Message}");
                Debug.LogError("广告显示失败: " + err.DetailMessage);
            }
        });
    }

    public void OnShare()
    {
        Debug.LogWarning("OnShare called");

        string localPath = Path.Combine(Application.streamingAssetsPath, "sharePicture.png");

        var opts = new OmniSDKShareOptions
        {
            platform = OmniSDKSharePlatform.SYSTEM,
            title = "Mock Title",
            description = "Mock Description",
            linkUrl = localPath,
            imageUrl = "https://cn.bing.com/th?id=OHR.CERNCenter_EN-US9854867489_1920x1080.jpg"
        };

        OmniSDK.Share(opts, r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Debug.Log("分享成功");
                Toast.Show("分享成功");
            }
            else
            {
                var err = r.GetError();
                Toast.Show("分享失败：" + err.Message);
                Debug.LogError("分享失败: " + err.DetailMessage);
            }
        });
    }

    public void OnCaptureMessage()
    {
        Debug.LogWarning("OnCaptureMessage called");

        var opts = new OmniSDKCaptureOptions
        {
            payload = "Unity test message for sentry capture.",
            level = OmniSDKCaptureLevel.ERROR,
            extraData = new Dictionary<string, string> {
                {"StackTrace", "Stack trace information."},
                {"Other", "Other data."}
            }
        };

        OmniSDK.Capture(opts);
        Toast.Show("已提交消息至 Sentry");
    }

    public void OnOpenAccountCenter()
    {
        Debug.LogWarning("OnOpenAccountCenter called");

        OmniSDK.OpenAccountCenter();
    }

    public void OnDeleteAccount()
    {
        Debug.LogWarning("OnDeleteAccount called");

        var opts = new OmniSDKDeleteAccountOptions
        {
            enableCustomUI = false
        };
        OmniSDK.DeleteAccount(opts, r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Toast.Show("账号删除成功: UserId - " + result.userId);
                Debug.LogWarning("账号删除成功: UserId - " + result.userId);
            }
            else
            {
                var err = r.GetError();
                Toast.Show("账号删除失败: " + err.Message);
                Debug.LogError("账号删除失败: " + err.DetailMessage);
            }
        });
    }
}
