using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 负责开启HOST，开启客户端，处理客户端连接和断连，客户端连接允许和摧毁和生成厨房物体
/// </summary>
public class KitchenGameMultiplayer : NetworkBehaviour
{
    public const int MAX_PLAYER_AMOUNT = 4;
    public const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static KitchenGameMultiplayer Instance { get; private set; }

    /// <summary>
    /// 该事件目前用于显示和隐藏ConnectingUI
    /// </summary>
    public event EventHandler OnTryingToJoinGame;
    /// <summary>
    /// 该事件目前用于显示和隐藏连接信息UI
    /// </summary>
    public event EventHandler OnFailToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;

    private string playerName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER,"PlayerName" + UnityEngine.Random.Range(100,1000));//获取玩家名，如果不存在返回默认值
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this,EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }
    /// <summary>
    /// 从列表中移除已断连客户端的玩家数据
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                //断开连接
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,//收集连接的客户端ID加入到网络列表的“玩家数据”结构体中
            colorId = GetFirstUnusedColorId(),//给每个新加入的客户端的玩家设置默认颜色
            //playerName = this.playerName,//为什么不在这里设置玩家名字？
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())//如果不在角色选择界面加入游戏就不可以再加入游戏了
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }
        connectionApprovalResponse.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerIndex];//server要获取playerData，不要用成GetPlayerData()这个是返回当前客户端的
        playerData.playerId = playerId;
        playerDataNetworkList[playerIndex] = playerData;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        //获取发送该RPC是哪个玩家
        int playerIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        //不可直接修改，先获取该玩家的玩家数据
        PlayerData playerData = playerDataNetworkList[playerIndex];//server要获取playerData，不要用成GetPlayerData()这个是返回当前客户端的

        //修改玩家名字
        playerData.playerName = playerName;

        //修改后的结构体重新放回列表
        playerDataNetworkList[playerIndex]= playerData;
    }

    /// <summary>
    /// 显示和隐藏客户端加入失败对应的UI
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailToJoinGame?.Invoke(this, EventArgs.Empty);
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


    [ServerRpc(RequireOwnership = false)]//所有客户端都可以发送请求
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)//可以把GameObject和NetworkObject安全传入RPC
    {
        //从索引获取厨房物体SO
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        #region 在引用中找到当时传进来的NetworkObject并获取上面的脚本组件
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);//传进来的NetworkObject就是厨房物体父对象的NetworkObject，不是玩家就是柜台
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();//IKitchenObjectParen组件是用来给厨房物体设置父对象的
        #endregion

        if (kitchenObjectParent.HasKitchenObject())//延迟导致的父对象已经有厨房物体了，便不再生成以及设定父对象
        {
            return;
        }
        //根据SO生成厨房物体预制体
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);

        #region 根据预制体在网络上生成网络物体（同步生成物体）
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);
        #endregion

        #region 给厨房物体设置父对象
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();//根据预制体获取厨房物体组件


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

        if(kitchenObjectNetworkObject == null)//由于延迟，网络物体已被摧毁
        {
            return;
        }

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

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        //检查网络列表中是否有该玩家的索引（该玩家索引是否连接成功）
        return playerIndex < playerDataNetworkList.Count;//按顺序添加的如果该索引小于列表长度说明一定连接成功了
    }

    /// <summary>
    /// 根据客户端ID获取玩家索引（第几个玩家）
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }
    /// <summary>
    /// 根据本地客户端ID获取该ID客户端的玩家数据
    /// </summary>
    /// <returns></returns>
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        //检测该颜色ID（某个点击了按钮的客户端）的颜色是否被使用了
        if (!IsColorAvailable(colorId))
        {
            return;
        }
        //该颜色未被使用，更新这个客户端（点击了按钮的客户端）的玩家数据中的颜色ID：
        //明确发送该RPC的客户端的玩家是第几个玩家
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        //无法直接更改自定义结构体的成员的值↓（netcode提示非变量，无法直接修改）
        //playerDataNetworkList[playerDataIndex].clientId = colorId;

        //根据索引先获取玩家数据的结构体
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        
        //修改成员
        playerData.colorId = colorId;

        //将更改后的结构体覆盖到列表中
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    /// <summary>
    /// 检查是否有其他玩家使用了该颜色
    /// </summary>
    /// <param name="colorId"></param>
    /// <returns></returns>
    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)//遍历所有连接的玩家数据
        {
            if (playerData.colorId == colorId)//寻找所有玩家数据中是否有颜色ID跟传进来的颜色ID一致
            {
                return false;
            }
        }
        return true;
    }

    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }
}
