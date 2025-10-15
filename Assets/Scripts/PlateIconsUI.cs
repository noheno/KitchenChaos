using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 功能：盘子上已经有的物体，生成其对应图标
/// </summary>
public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in transform)
        {
            if (child == iconTemplate)
            {
                continue;
            }
            Destroy(child.gameObject);
        }
        //获取盘子列表（盘子上都有哪些东西）
        foreach (KitchenObjectSO kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
        {
            //生成图标，另外父对象被取消激活了（Awake中）↑
            Transform iconTransform = Instantiate(iconTemplate, transform);//父对象要为PlateIconUI，才能保证生成的图标为world object（何意味？）
            //激活复制的预制体
            iconTransform.gameObject.SetActive(true);
            //更改图像
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}
