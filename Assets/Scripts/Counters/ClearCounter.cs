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
        //�����̨��û��������
        if (!HasKitchenObject())
        {
            //�������������
            if (player.HasKitchenObject())
            {
                Drop(player);
            }
            else
            {
                //�������û���������ڵ���
            }
        }
        //�����̨���г�������
        else
        {
            //��������г�������
            if (player.HasKitchenObject())
            {
                #region �����������ϵ�������
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    #region �Ѷ����ӹ�̨�Ϸŵ�������е�������
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                    #endregion
                }
                #endregion

                #region ���������ϵĲ������Ӷ�����������
                else
                {
                    #region �����̨�ϵ�������
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        #region �Ѷ�����������Ϸŵ�������
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            //player.GetKitchenObject().SetKitchenObjectParent();
                            player.GetKitchenObject().DestroySelf();
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion


            }
            else//���û�г�������
            {
                Pick(player);
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
