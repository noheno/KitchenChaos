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
    /// 顾客所需菜品等待列表
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
        #region 随机生成食谱-生成脚本化对象并传送至客户端
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
    /// 接收传输过来的食谱，加入到列表中并触发生成食谱事件
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
    /// 递交判断
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            //先检查递交时的食材数量对不对
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                #region 检查递交时的成分对不对
                //遍历生成食谱的所有食材
                foreach (KitchenObjectSO kitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    //遍历盘子上的所有食材
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (kitchenObjectSO == plateKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)//未找到对应成分
                    {
                        plateContentsMatchesRecipe = false;//成分不匹配
                    }
                }
                if (plateContentsMatchesRecipe)//成分匹配
                {
                    //玩家递交了正确的菜品
                    int waitingRecipeSOListIndex = i;
                    Debug.Log("玩家递交了正确的菜品");
                    DeliveryCorrectRecipeServerRpc(waitingRecipeSOListIndex);
                    return;//返回，不再执行该函数
                }
                #endregion
            }
        }
        //食谱清单中没有玩家递交的菜品
        //玩家递交了错误的菜品
        DeliveryInCorrectRecipeServerRpc();
    }

    /// <summary>
    /// 同步到所有客户端
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
    /// 获取系统随机生成的食谱列表
    /// </summary>
    /// <returns></returns>
    public List<RecipeSO> GetWaitingRecipeSOList() => waitingRecipeSOList;

    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;

}
