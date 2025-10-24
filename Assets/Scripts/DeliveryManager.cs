using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    [SerializeField] private RecipeListSO recipeListSO;
    /// <summary>
    /// �˿������Ʒ�ȴ��б�
    /// </summary>
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 4;
    private int waitingRecipeMax = 4;
    private int successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
        successfulRecipesAmount = 0;
    }

    private void Update()
    {
        if (!IsServer) { return; }
        spawnRecipeTimer -= Time.deltaTime;
        #region �������ʳ��-���ɽű������󲢴������ͻ���
        if (spawnRecipeTimer <= 0)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipeMax)
            {
                int waitingRecipeSOIIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIIndex];
                SpawnNetWaitingRecipeClientRpc(waitingRecipeSOIIndex);
            }
        }
        #endregion
    }

    /// <summary>
    /// ���մ��������ʳ�ף����뵽�б��в���������ʳ���¼�
    /// </summary>
    /// <param name="waitingRecipeSO"></param>
    [ClientRpc]
    private void SpawnNetWaitingRecipeClientRpc(int waitingRecipeSOIIndex)
    {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIIndex];
        waitingRecipeSOList.Add(waitingRecipeSO);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// �ݽ��ж�
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            //�ȼ��ݽ�ʱ��ʳ�������Բ���
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                #region ���ݽ�ʱ�ĳɷֶԲ���
                //��������ʳ�׵�����ʳ��
                foreach (KitchenObjectSO kitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    //���������ϵ�����ʳ��
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (kitchenObjectSO == plateKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)//δ�ҵ���Ӧ�ɷ�
                    {
                        plateContentsMatchesRecipe = false;//�ɷֲ�ƥ��
                    }
                }
                if (plateContentsMatchesRecipe)//�ɷ�ƥ��
                {
                    //��ҵݽ�����ȷ�Ĳ�Ʒ
                    int waitingRecipeSOListIndex = i;
                    Debug.Log("��ҵݽ�����ȷ�Ĳ�Ʒ");
                    DeliveryCorrectRecipeServerRpc(waitingRecipeSOListIndex);
                    return;//���أ�����ִ�иú���
                }
                #endregion
            }
        }
        //ʳ���嵥��û����ҵݽ��Ĳ�Ʒ
        //��ҵݽ��˴���Ĳ�Ʒ
        DeliveryInCorrectRecipeServerRpc();
    }

    /// <summary>
    /// ͬ�������пͻ���
    /// </summary>
    /// <param name="waitingRecipeSOListIndex"></param>
    [ServerRpc(RequireOwnership = false)]
    private void DeliveryCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliveryCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliveryCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        successfulRecipesAmount++;
        waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliveryInCorrectRecipeServerRpc()
    {
        DeliveryInCorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliveryInCorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ��ȡϵͳ������ɵ�ʳ���б�
    /// </summary>
    /// <returns></returns>
    public List<RecipeSO> GetWaitingRecipeSOList() => waitingRecipeSOList;

    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;

}
