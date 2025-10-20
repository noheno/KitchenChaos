using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 盘子，继承自厨房物体，功能有：1.判断是否可以并尝试把不同地方的东西到盘子上；2.返回盘子上的物品列表
/// </summary>
public class PlateKitchenObject : KitchenObject
{
    /// <summary>
    /// 事件“可放入盘子物体加入到盘子列表中”
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
    /// 把所有能放到盘子上的东西加到列表中
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))//如果该物体不是有效物体（不可以放在盘子上）
        {
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectSO))//如果列表中已经有了该厨房物体（盘子上只能放一个同类型的食材）
        {
            return false;
        }
        kitchenObjectSOList.Add(kitchenObjectSO);

        //如果该物体可以加入到盘子中则也加到消息类中
        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs()
        {
            kitchenObjectSO = kitchenObjectSO
        });
        return true;
    }
    /// <summary>
    /// 返回盘子上的所有食材
    /// </summary>
    /// <returns></returns>
    public List<KitchenObjectSO> GetKitchenObjectSOList() => kitchenObjectSOList;

}
