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

    /// <summary>
    /// ���ɳ������壬�贫������������ͺͳ�������ĸ�����
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <param name="kitchenObjectParent"></param>
    /// <returns></returns>
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        int index = GetKitchenObjectSOIndex(kitchenObjectSO);
        SpawnKitchenObjectServerRpc(index, kitchenObjectParent.GetNetworkObject());
    }


    [ServerRpc(RequireOwnership = false)]//�ͻ���Ҳ���Ե���(��������)
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)//���԰�GameObject��NetworkObject��ȫ����RPC
    {
        #region ��������ȡ��������SO������SO���ɳ�������Ԥ����
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        #endregion

        #region ����Ԥ�����������������������壨ͬ���������壩
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);
        #endregion

        #region �������������ø�����
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();//����Ԥ�����ȡ�����������

        #region ���������ҵ���ʱ��������NetworkObject����ȡ����Ľű����
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);//��������NetworkObject���ǳ������常�����NetworkObject��������Ҿ��ǹ�̨
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();//IKitchenObjectParen����������������������ø������
        #endregion

        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
        #endregion

    }

    #region client rpc
    //[ClientRpc]
    //private void SetKitchenObjectParentClientRpc(Transform transform, NetworkObjectReference reference)
    //{
    //    #region �������������ø�����
    //    KitchenObject kitchenObject = transform.GetComponent<KitchenObject>();//����Ԥ�����ȡ�����������

    //    #region �������û�ȡ���͵��������岢��ȡ�����������ĵġ��������常�������
    //    reference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);//TryGet�ڲ���һ�����Ҳ�����ͨ�� ID �ڹ����б��в��Ҷ���
    //    IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
    //    #endregion

    //    kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    //    #endregion
    //}
    #endregion

    private int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    private KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

}
