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
        //如果玩家手上有物体且柜台上没物体，交互后放在柜台上
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                Drop(player);
            }
            else
            {
                //玩家手上没有物体用于掉落
            }
        }
        //如果玩家手上无物体且柜台上有物体，交互后放到玩家手上
        else
        {
            if (!player.HasKitchenObject())
            {
                Pick(player);
            }
            else
            {
                //玩家手上已经有物品了，不可捡起
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
