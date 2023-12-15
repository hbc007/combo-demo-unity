using System;
using EasyUI.Toast;
using Seayoo.OmniSDK;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePanel : MonoBehaviour
{
    public InputField productIdField;
    public InputField productPriceField;
    public Button purchaseBtn;
    public Button cancelBtn;
    // Start is called before the first frame update
    void Start()
    {
        purchaseBtn.onClick.AddListener(OnPurchase);
        cancelBtn.onClick.AddListener(OnCancel);
    }
    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void OnPurchase()
    {
        var opts = new OmniSDKPurchaseOptions
        {
            productId = productIdField.text,
            productType = OmniSDKProductType.INAPP,
            productName = "60钻石",
            productDesc = "购买60钻石畅玩游戏",
            productUnitPrice = Convert.ToDouble(productPriceField.text),
            purchaseAmount = Convert.ToDouble(productPriceField.text),
            purchaseQuantity = 1,
            purchaseCallbackUrl = "https://a2.xgsdk.dev.seayoo.com/mock/recharge/notify",
            gameOrderId = GenOrderId(),
            gameRoleId = "123456",
            gameRoleName = "Tester",
            gameRoleLevel = "1",
            gameRoleVipLevel = "1",
            gameCurrencyUnit = "魔石",
            gameZoneId = "z1",
            gameServerId = "s1",
            currency = "CNY",
            extJson = "{}",
        };
        Debug.Log("开始购买: GameOrderId - " + opts.gameOrderId);
        OmniSDK.Purchase(opts, r =>
        {
            if (r.IsSuccess())
            {
                var result = r.Get();
                Toast.Show("购买完成：OrderId - " + result.orderId);
                Debug.Log("购买完成：" + result.ToString());
            }
            else
            {
                var err = r.GetError();
                Toast.Show("购买失败：" + err.Message);
                Debug.LogError("购买失败：" + err.DetailMessage);
            }
        });
    }

    private void OnCancel()
    {
        this.gameObject.SetActive(false);
    }

    private string GenOrderId()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        System.Random random = new System.Random();

        for (int i = 0; i < 16; i++)
        {
            sb.Append(random.Next(0, 10));
        }

        return sb.ToString();
    }
}
