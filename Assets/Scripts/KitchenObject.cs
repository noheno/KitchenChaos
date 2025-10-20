using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1.������ʳ��Ԥ�����ϵĽű������ڷ��ظ�ʳ�ĵ����ͣ����������ֽű�������
/// 2.�趨��ʳ��Ӧ������ָ����������
/// </summary>
public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    /// <summary>
    /// ��ǰ������Ʒ�ĸ�����
    /// </summary>
    private IKitchenObjectParent kitchenObjectParent;

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
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
        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();//�޸ĳ�����Ʒ�ĸ�����
        transform.localPosition = Vector3.zero;
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
    public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO,IKitchenObjectParent kitchenObjectParent)
    {
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
        return kitchenObject;
    }

}
