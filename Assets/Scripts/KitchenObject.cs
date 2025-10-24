using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1.挂载在食材预制体上的脚本，用于返回该食材的类型（返回是哪种脚本化对象）
/// 2.设定该食材应放置在指定父对象上
/// </summary>
public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    /// <summary>
    /// 当前厨房物品的父对象
    /// </summary>
    private IKitchenObjectParent kitchenObjectParent;
    /// <summary>
    /// 多人游戏的厨房物体跟随点
    /// </summary>
    private FollowTransform followTransform;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    /// <summary>
    /// 设定厨房物体父对象（现已改为仅在Client上运行）
    /// </summary>
    /// <param name="kitchenObjectParent"></param>
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());//告知服务器本厨房物体需要这个新的parent
    }

    /// <summary>
    /// 广播给所有客户端厨房物体更新了parent
    /// </summary>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    /// <summary>
    /// 设定厨房物体父对象（网络版）
    /// </summary>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        #region 在引用中找到当时传进来的NetworkObject并获取上面的脚本组件
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        #endregion

        #region 单机版设定厨房物体父对象
        //第二次设定父对象
        if (this.kitchenObjectParent != null)
        {
            //告诉原来父对象自己不在了，清除原来父对象的厨房物品
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;//给厨房物品设置当前父对象
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("厨房物品的父对象已经有厨房物品了");
        }
        kitchenObjectParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
        #endregion
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return this.kitchenObjectParent;
    }

    public void DestroySelf()
    {
        kitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }
    /// <summary>
    /// 判断是否为该厨房物体是否为盘子
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    /// <returns></returns>
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)//如果该厨房物体是盘子
        {
            plateKitchenObject = this as PlateKitchenObject;//输出参数赋值成盘子
            return true;
        }
        else
        {
            plateKitchenObject= null;
            return false;
        }
    }

    /// <summary>
    /// 生成厨房物体，需传入厨房物体类型和厨房物体的父对象
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <param name="kitchenObjectParent"></param>
    /// <returns></returns>
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO,IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

}
