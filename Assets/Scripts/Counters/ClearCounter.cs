using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class ClearCounter : BaseCounter
{
    /// <summary>
    /// ���ڸù�̨���ɵ�ʳ������
    /// </summary>
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    private void Update()
    {

    }
    
    /// <summary>
    /// ClearCounter�޷����ɳ������壬ֻ�ܸ��������ͷ��³�������
    /// </summary>
    /// <param name="player"></param>
    public override void Interact(Player player)
    {
        //�����������������ҹ�̨��û���壬��������ڹ�̨��
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                Drop(player);
            }
            else
            {
                //�������û���������ڵ���
            }
        }
        //�����������������ҹ�̨�������壬������ŵ��������
        else
        {
            if (!player.HasKitchenObject())
            {
                Pick(player);
            }
            else
            {
                //��������Ѿ�����Ʒ�ˣ����ɼ���
            }
        }
    }

    private void Pick(Player player)
    {
        GetKitchenObject().SetKitchenObjectParent(player);
    }

    private void Drop(Player player)
    {
        player.GetKitchenObject().SetKitchenObjectParent(this);
    }
}
