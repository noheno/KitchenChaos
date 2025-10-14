using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    /// <summary>
    /// 生成食材的位置
    /// </summary>
    [SerializeField] private Transform counterTopPoint;
    /// <summary>
    /// 当前柜台上的厨房物体
    /// </summary>
    private KitchenObject kitchenObject;
    public virtual void Interact(Player player)
    {
        Debug.Log("BaseCounter.Interact();");
    }
    public virtual void InteractAlternate(Player player)
    {
        //Debug.Log("BaseCounter.InteractAlternate();");
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject() { return kitchenObject; }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject() => kitchenObject != null;


}
