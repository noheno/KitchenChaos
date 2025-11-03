using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }
    private Dictionary<ulong, bool> playerReadyDictionary;

    public event EventHandler OnReadyChanged;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //向所有客户端广播该客户端已经加入字典
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

        // 标志所有客户端是否已准备好  
        bool allClientsReady = true;

        // 将当前客户端标记为已准备  
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        // 遍历所有连接到服务器的客户端ID  
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // 玩家不存在或玩家没准备好
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // 不能开始倒计时
                allClientsReady = false;
                break;
            }
        }
        // 所有客户端已准备则加载至游戏场景
        if (allClientsReady)
        {
            KitchenGameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }

}
