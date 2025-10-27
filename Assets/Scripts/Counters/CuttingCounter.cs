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
    /// Ӧ��������һ������OnCut�¼�ʱ�������¼�
    /// </summary>
    public static event EventHandler OnAnyCut;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;

    new public static void ResetStaticData()
    {
        OnAnyCut = null;//������м���
    }


    public override void Interact(Player player)
    {
        //��̨��û����
        if (!HasKitchenObject())
        {
            //�������������
            if (player.HasKitchenObject())
            {
                //�Ҹ�����Ϊ���и�����
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();//��ʳ�ķŵ���̨��
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlaceObjectOnCounterServerRpc();
                }
            }
            else
            {

            }
        }
        //������ŵ��������
        else//��̨��������
        {
            //����������������
            if (player.HasKitchenObject())
            {
                #region ���������ϵ�������
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    #region �Ѷ����ŵ�������
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                    #endregion
                }
                #endregion
            }
            //����������������
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;
        #region �����¼����и���ȷ����仯��������Ϣ����ǰ�и���ȡ�
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = 0f
        });
        #endregion
    }

    public override void InteractAlternate(Player player)
    {
        //��̨���г��������ҳ�������Ϊ���и���Ʒ
        if (HasKitchenObject()&& HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc() 
    {
        cuttingProgress++;
        #region �����¼����и���ȷ����仯��������Ϣ����ǰ�и���ȡ�
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax,
        });
        #endregion

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)//�����ڷ�����������
        {
            KitchenObjectSO output = GetOutputFormInput(GetKitchenObject().GetKitchenObjectSO());
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
            KitchenObject.SpawnKitchenObject(output, this);
        }
    }


    /// <summary>
    /// �жϸó�����Ʒ�Ƿ���Ա��и�
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
