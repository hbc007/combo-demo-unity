#if UNITY_STANDALONE
using EasyUI.Toast;
using Seayoo.OmniSDK.Windows;
using UnityEngine;

public class OmniSDKListenerImpl : OmniSDKListener
{
    public override void OnStart(OmniSDKResult<OmniSDKStartResult> result)
    {
        if (result.IsSuccess())
        {
            var startResult = result.Get();
            var appId = startResult.appId;
            var planId = startResult.planId;
            var sdkVersion = startResult.sdkVersion;
            Debug.LogWarning($"OmniSDKListenerImpl.OnStart, appId = {appId}, planId = {planId}, sdkVersion = {sdkVersion}");
            Demo.isInit = true;
        }
        else
        {
            Debug.LogError(result.GetError().ToString());
            // Toast.Show(result.GetError().ToString());
        }
    }

    public override void OnLogin(OmniSDKResult<OmniSDKLoginResult> result)
    {
        // Handle OnLogin
        if (result.IsSuccess())
        {
            var loginResult = result.Get();
            var userId = loginResult.loginInfo.userId;
            var channelId = loginResult.loginInfo.channelId;
            var signature = loginResult.loginInfo.signature;
            var authTime = loginResult.loginInfo.authTime;
            Debug.LogWarning($"{userId},{channelId},{signature},{authTime}");
            Toast.Show("登录成功");
        }
        else
        {
            Debug.LogError(result.GetError().ToString());
            // Toast.Show(result.GetError().ToString());
        }
    }
}
#endif