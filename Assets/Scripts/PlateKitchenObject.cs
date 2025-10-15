using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    /// <summary>
    /// �¼����ɷ�������������뵽�����б��С�
    /// </summary>
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }


    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    private List<KitchenObjectSO> kitchenObjectSOList;
    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    /// <summary>
    /// �������ܷŵ������ϵĶ����ӵ��б���
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))//��������岻����Ч���壨�����Է��������ϣ�
        {
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectSO))//����б����Ѿ����˸ó������壨������ֻ�ܷ�һ��ͬ���͵�ʳ�ģ�
        {
            return false;
        }
        kitchenObjectSOList.Add(kitchenObjectSO);

        //�����������Լ��뵽��������Ҳ�ӵ���Ϣ����
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs()
        {
            kitchenObjectSO = kitchenObjectSO
        });
        return true;
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList() => kitchenObjectSOList;

}
