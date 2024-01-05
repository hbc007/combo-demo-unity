using Combo;
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
    void Start()
    {
        var setupOptions = new ComboSDKSetupOptions
        {
            gameId = "demo",
            publishableKey = "demo_publishable_key"
        };

        ComboSDK.Setup(setupOptions, r =>
        {
            if (r.IsSuccess)
            {
                var result = r.Data;
                Debug.Log($"初始化成功: GAMDID - {result.gameId}; DISTRO - {result.distro}; SDKVERSION - {result.sdkVersion}; PLATFORMVERSION - {result.platformVersion}");
            }
            else
            {
                var err = r.Error;
                Toast.Show($"初始化失败：{err.Message}");
                Debug.LogError("初始化失败: " + err.DetailMessage);
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
                if (error.Error == ComboSDKErrorTypes.UserCancelled)
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

        var opts = new ComboSDKPreloadAdOptions
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
        var opts = new ComboSDKShowAdOptions
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

    public void OnShare()
    {
    }

    public void OnCaptureMessage()
    {
    }

    public void OnOpenAccountCenter()
    {
    }

    public void OnDeleteAccount()
    {
    }
}
