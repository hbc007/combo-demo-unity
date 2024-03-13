using System;
using System.Collections;
using System.Collections.Generic;
using Combo;
using UnityEngine;
using UnityEngine.UI;
using EasyUI.Toast;

public class DownloadUrlPanel : MonoBehaviour
{
    public HyperlinkText downloadUrlTxt;

    public void Show(string downloadUrl){
        this.gameObject.SetActive(true);
        downloadUrlTxt.text = $"<a href={downloadUrl}>{downloadUrl}</a>";
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    public void Copy()
    {
        UnityEngine.GUIUtility.systemCopyBuffer = downloadUrlTxt.text;
        Toast.Show("复制成功");
    }
}
