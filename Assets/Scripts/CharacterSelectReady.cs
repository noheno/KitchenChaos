using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }
    private Dictionary<ulong, bool> playerReadyDictionary;

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
        // ��־���пͻ����Ƿ���׼����  
        bool allClientsReady = true;

        // ����ǰ�ͻ��˱��Ϊ��׼��  
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        // �����������ӵ��������Ŀͻ���ID  
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // ��Ҳ����ڻ����û׼����
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // ���ܿ�ʼ����ʱ
                allClientsReady = false;
                break;
            }
        }
        // ���пͻ�����׼�����������Ϸ����
        if (allClientsReady)
        {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

}
