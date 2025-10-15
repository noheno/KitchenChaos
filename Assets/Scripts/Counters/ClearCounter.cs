using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class ClearCounter : BaseCounter
{
    /// <summary>
    /// 会在该柜台生成的食材类型
    /// </summary>
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    private void Update()
    {

    }
    
    /// <summary>
    /// ClearCounter无法生成厨房物体，只能给玩家拿起和放下厨房物体
    /// </summary>
    /// <param name="player"></param>
    public override void Interact(Player player)
    {
        //如果柜台上没厨房物体
        if (!HasKitchenObject())
        {
            //玩家手上有物体
            if (player.HasKitchenObject())
            {
                Drop(player);
            }
            else
            {
                //玩家手上没有物体用于掉落
            }
        }
        //如果柜台上有厨房物体
        else
        {
            //玩家手上有厨房物体
            if (player.HasKitchenObject())
            {
                #region 且如果玩家手上的是盘子
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    #region 把东西从柜台上放到玩家手中的盘子上
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                    #endregion
                }
                #endregion

                #region 如果玩家手上的不是盘子而是其他物体
                else
                {
                    #region 如果柜台上的是盘子
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        #region 把东西从玩家手上放到盘子上
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
            else//玩家没有厨房物体
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
