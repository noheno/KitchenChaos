using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenGameMultiplayer : NetworkBehaviour
{
    public static KitchenGameMultiplayer Instance { get; private set; }
    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    private void Awake()
    {
        Instance = this;
    }

    #region 生成厨房物体
    /// <summary>
    /// 生成厨房物体，需传入厨房物体类型和厨房物体的父对象
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <param name="kitchenObjectParent"></param>
    /// <returns></returns>
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        int index = GetKitchenObjectSOIndex(kitchenObjectSO);
        SpawnKitchenObjectServerRpc(index, kitchenObjectParent.GetNetworkObject());
    }


    [ServerRpc(RequireOwnership = false)]//所有客户端都可以调用(发送请求)
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)//可以把GameObject和NetworkObject安全传入RPC
    {
        #region 从索引获取厨房物体SO，根据SO生成厨房物体预制体
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        #endregion

        #region 根据预制体在网络上生成网络物体（同步生成物体）
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);
        #endregion

        #region 给厨房物体设置父对象
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();//根据预制体获取厨房物体组件

        #region 在引用中找到当时传进来的NetworkObject并获取上面的脚本组件
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);//传进来的NetworkObject就是厨房物体父对象的NetworkObject，不是玩家就是柜台
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();//IKitchenObjectParen组件是用来给厨房物体设置父对象的
        #endregion

        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
        #endregion

    }

    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }
    #endregion

    #region 摧毁厨房物体
    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjecServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjecServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        ClearKitchenObjectParentClientRpc(kitchenObjectNetworkObjectReference);
        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        kitchenObject.ClearKitchenObjectParent();
    }

    #endregion
}
