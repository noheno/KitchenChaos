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
        //��̨��û����
        if (!HasKitchenObject())
        {
            //�������������
            if (player.HasKitchenObject())
            {
                //�Ҹ�����Ϊ���и�����
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);//��ʳ�ķŵ���̨��
                    cuttingProgress = 0;

                    #region �����¼����и���ȷ����仯��������Ϣ����ǰ�и���ȡ�
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
                //�������û��������Է��ڸù�̨��
                return;
            }
        }
        //�����������������ҹ�̨�������壬������ŵ��������
        else
        {
            if (!player.HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else
            {
                //��������Ѿ�����Ʒ�ˣ����ɼ���
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        //��̨���г��������ҳ�������Ϊ���и���Ʒ
        if (HasKitchenObject()&& HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            cuttingProgress++;
            #region �����¼����и���ȷ����仯��������Ϣ����ǰ�и���ȡ�
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
