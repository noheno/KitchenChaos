using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置颜色按钮，给玩家设置该按钮的颜色
/// </summary>
public class CharacterColorSelectSingleUI : MonoBehaviour
{
    /// <summary>
    /// 手动设置的ID，用于标记要使用服务器的颜色列表中的第几个颜色
    /// </summary>
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;


    private void Awake()
    {
        //每次点击按钮修改点击该按钮的客户端的玩家的颜色
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.ChangePlayerColor(colorId);//更改玩家的颜色索引为按钮颜色索引
        });
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        image.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);//给按钮从左到右按照颜色列表中的顺序设置颜色
        UpdateIsSelected();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();//每次点击按钮更新按钮选中特效
    }

    /// <summary>
    /// 更新按钮选中特效
    /// </summary>
    private void UpdateIsSelected()
    {
        if (KitchenGameMultiplayer.Instance.GetPlayerData().colorId == colorId)//如果某个客户端的颜色ID(即玩家颜色)和该颜色按钮的ID一致
        {
            selectedGameObject.SetActive(true);//该按钮显示选中特效
        }
        else
        {
            selectedGameObject.SetActive(false);//其余情况下该按钮隐藏显示特效
        }
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }

}
