using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ܣ��������Ѿ��е����壬�������Ӧͼ��
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
        //��ȡ�����б������϶�����Щ������
        foreach (KitchenObjectSO kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
        {
            //����ͼ�꣬���⸸����ȡ�������ˣ�Awake�У���
            Transform iconTransform = Instantiate(iconTemplate, transform);//������ҪΪPlateIconUI�����ܱ�֤���ɵ�ͼ��Ϊworld object������ζ����
            //����Ƶ�Ԥ����
            iconTransform.gameObject.SetActive(true);
            //����ͼ��
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}
