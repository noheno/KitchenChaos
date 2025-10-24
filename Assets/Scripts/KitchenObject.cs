using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1.������ʳ��Ԥ�����ϵĽű������ڷ��ظ�ʳ�ĵ����ͣ����������ֽű�������
/// 2.�趨��ʳ��Ӧ������ָ����������
/// </summary>
public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    /// <summary>
    /// ��ǰ������Ʒ�ĸ�����
    /// </summary>
    private IKitchenObjectParent kitchenObjectParent;
    /// <summary>
    /// ������Ϸ�ĳ�����������
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
    /// �趨�������常�������Ѹ�Ϊ����Client�����У�
    /// </summary>
    /// <param name="kitchenObjectParent"></param>
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());//��֪������������������Ҫ����µ�parent
    }

    /// <summary>
    /// �㲥�����пͻ��˳������������parent
    /// </summary>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    /// <summary>
    /// �趨�������常��������棩
    /// </summary>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        #region ���������ҵ���ʱ��������NetworkObject����ȡ����Ľű����
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        #endregion

        #region �������趨�������常����
        //�ڶ����趨������
        if (this.kitchenObjectParent != null)
        {
            //����ԭ���������Լ������ˣ����ԭ��������ĳ�����Ʒ
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;//��������Ʒ���õ�ǰ������
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("������Ʒ�ĸ������Ѿ��г�����Ʒ��");
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
    /// �ж��Ƿ�Ϊ�ó��������Ƿ�Ϊ����
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    /// <returns></returns>
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)//����ó�������������
        {
            plateKitchenObject = this as PlateKitchenObject;//���������ֵ������
            return true;
        }
        else
        {
            plateKitchenObject= null;
            return false;
        }
    }

    /// <summary>
    /// ���ɳ������壬�贫������������ͺͳ�������ĸ�����
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <param name="kitchenObjectParent"></param>
    /// <returns></returns>
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO,IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

}
