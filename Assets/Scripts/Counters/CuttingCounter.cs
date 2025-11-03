using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter,IHasProgress
{
    [SerializeField] private CuttingRecipeSO[] cutRecipeSOArray;
    private int cuttingProgress;
    /// <summary>
    /// 应该在任意一个触发OnCut事件时触发该事件
    /// </summary>
    public static event EventHandler OnAnyCut;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    new public static void ResetStaticData()
    {
        OnAnyCut = null;//清除所有监听
    }


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
                    KitchenObject kitchenObject = player.GetKitchenObject();//将食材放到柜台上
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlaceObjectOnCounterServerRpc();
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
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
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

    /// <summary>
    /// 广播同步重置切割进度
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    /// <summary>
    /// 重置切割进度（应在每当玩家把东西放到柜台上触发）
    /// </summary>
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;
        #region 触发事件“切割进度发生变化”附带消息“当前切割进度”
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
        #endregion
    }

    public override void InteractAlternate(Player player)
    {
        //柜台上有厨房物体且厨房物体为可切割物品
        if (HasKitchenObject()&& HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    /// <summary>
    /// 广播同步执行切割进度
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        //如果延迟太高就跳过这次执行
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectClientRpc();
        }
    }

    /// <summary>
    /// 执行切割过程
    /// </summary>
    [ClientRpc]
    private void CutObjectClientRpc() 
    {
        cuttingProgress++;
        #region 触发事件“切割进度发生变化”附带消息“当前切割进度”
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
        });
        #endregion

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 切割进度判断（只在服务器上运行）
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)//限制在服务器上运行
            {
                KitchenObjectSO output = GetOutputFormInput(GetKitchenObject().GetKitchenObjectSO());
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
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
