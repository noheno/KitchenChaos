using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    [SerializeField] private CuttingRecipeSO[] cutRecipeSOArray;
    private int cuttingProgress;

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    public override void Interact(Player player)
    {
        //柜台上没物体
        if (!HasKitchenObject())
        {
            //玩家手上有物体
            if (player.HasKitchenObject())
            {
                //且该物体为可切割物体
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);//将食材放到柜台上
                    cuttingProgress = 0;

                    #region 触发事件“切割进度发生变化”附带消息“当前切割进度”
                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
                    });
                    #endregion
                }
            }
            else
            {

            }
        }
        //交互后放到玩家手上
        else//柜台上有物体
        {
            //如果玩家手上有物体
            if (player.HasKitchenObject())
            {
                #region 如果玩家手上的是盘子
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    #region 把东西放到盘子上
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();

                    }
                    #endregion
                }
                #endregion
            }
            //如果玩家手上无物体
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        //柜台上有厨房物体且厨房物体为可切割物品
        if (HasKitchenObject()&& HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            cuttingProgress++;
            #region 触发事件“切割进度发生变化”附带消息“当前切割进度”
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            OnProgressChanged?.Invoke(this,new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
            });
            #endregion
            OnCut.Invoke(this,EventArgs.Empty);
            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
            {
                KitchenObjectSO input = GetKitchenObject().GetKitchenObjectSO();
                GetKitchenObject().DestroySelf();
                KitchenObjectSO output = GetOutputFormInput(input);
                KitchenObject.SpawnKitchenObject(output, this);
            }
        }
    }

    /// <summary>
    /// 判断该厨房物品是否可以被切割
    /// </summary>
    /// <param name="inputKitchenObjectSO"></param>
    /// <returns></returns>
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputFormInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cutRecipeSOArray)
        {
            if (cuttingRecipeSO.input == kitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
