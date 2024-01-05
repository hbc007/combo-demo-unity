using EasyUI.Toast;
using UnityEngine;
using UnityEngine.UI;
using Combo;

public class PurchasePanel : MonoBehaviour
{
    public InputField productIdField;
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
        var opts = new PurchaseOptions
        {
            orderToken = productIdField.text
        };
        ComboSDK.Purchase(opts, r =>
        {
            purchaseBtn.interactable = true;
            if (r.IsSuccess)
            {
                var result = r.Data;
                Toast.Show("购买完成：OrderId - " + result.orderId);
                Debug.Log("购买完成：OrderId - " + result.orderId);
            }
            else
            {
                var err = r.Error;
                Toast.Show("购买失败：" + err.Message);
                Debug.LogError("购买失败：" + err.DetailMessage);
            }
        });
    }

    private void OnCancel()
    {
        this.gameObject.SetActive(false);
    }
}
