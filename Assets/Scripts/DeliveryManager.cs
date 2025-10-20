using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
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
    private float spawnRecipeTimer;
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
        spawnRecipeTimer -= Time.deltaTime;
        #region �������ʳ��
        if (spawnRecipeTimer <= 0)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipeMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                waitingRecipeSOList.Add(waitingRecipeSO);
                OnRecipeSpawned?.Invoke(this,EventArgs.Empty);
            }
        }
        #endregion
    }

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
                    Debug.Log("��ҵݽ�����ȷ�Ĳ�Ʒ");
                    successfulRecipesAmount++;
                    waitingRecipeSOList.RemoveAt(i);
                    OnRecipeCompleted?.Invoke(this,EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this,EventArgs.Empty);
                    return;//���أ�����ִ�иú���
                }
                #endregion
            }
        }
        //ʳ���嵥��û����ҵݽ��Ĳ�Ʒ
        //��ҵݽ��˴���Ĳ�Ʒ
        OnRecipeFailed?.Invoke(this,EventArgs.Empty);
    }

    /// <summary>
    /// ��ȡϵͳ������ɵ�ʳ���б�
    /// </summary>
    /// <returns></returns>
    public List<RecipeSO> GetWaitingRecipeSOList() => waitingRecipeSOList;

    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;

}
