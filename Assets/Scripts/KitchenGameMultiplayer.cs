using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// ������HOST�������ͻ��ˣ�����ͻ������ӺͶ������ͻ�����������ʹݻٺ����ɳ�������
/// </summary>
public class KitchenGameMultiplayer : NetworkBehaviour
{
    private const int MAX_PLAYER_AMOUNT = 4;
    public static KitchenGameMultiplayer Instance { get; private set; }

    /// <summary>
    /// ���¼�Ŀǰ������ʾ������ConnectingUI
    /// </summary>
    public event EventHandler OnTryingToJoinGame;
    /// <summary>
    /// ���¼�Ŀǰ������ʾ������������ϢUI
    /// </summary>
    public event EventHandler OnFailToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this,EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }
    /// <summary>
    /// ���б����Ƴ��Ѷ����ͻ��˵��������
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                //�Ͽ�����
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,//�ռ����ӵĿͻ���ID���뵽�����б�ġ�������ݡ��ṹ����
            colorId = GetFirstUnusedColorId()//��ÿ���¼���Ŀͻ��˵��������Ĭ����ɫ
        });
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())//������ڽ�ɫѡ����������Ϸ�Ͳ������ټ�����Ϸ��
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
        NetworkManager.Singleton.StartClient();
    }
    /// <summary>
    /// ��ʾ�����ؿͻ��˼���ʧ�ܶ�Ӧ��UI
    /// </summary>
    /// <param name="clientId"></param>
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    #region ���ɳ�������
    /// <summary>
    /// ���ɳ������壬�贫������������ͺͳ�������ĸ�����
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    /// <param name="kitchenObjectParent"></param>
    /// <returns></returns>
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        int index = GetKitchenObjectSOIndex(kitchenObjectSO);
        SpawnKitchenObjectServerRpc(index, kitchenObjectParent.GetNetworkObject());
    }


    [ServerRpc(RequireOwnership = false)]//���пͻ��˶����Ե���(��������)
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)//���԰�GameObject��NetworkObject��ȫ����RPC
    {
        #region ��������ȡ��������SO������SO���ɳ�������Ԥ����
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
        #endregion

        #region ����Ԥ�����������������������壨ͬ���������壩
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true);
        #endregion

        #region �������������ø�����
        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();//����Ԥ�����ȡ�����������

        #region ���������ҵ���ʱ��������NetworkObject����ȡ����Ľű����
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);//��������NetworkObject���ǳ������常�����NetworkObject��������Ҿ��ǹ�̨
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();//IKitchenObjectParen����������������������ø������
        #endregion

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

    #region �ݻٳ�������
    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjecServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjecServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
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
        //��������б����Ƿ��и���ҵ�����������������Ƿ����ӳɹ���
        return playerIndex < playerDataNetworkList.Count;//��˳����ӵ����������С���б���˵��һ�����ӳɹ���
    }

    /// <summary>
    /// ���ݿͻ���ID��ȡ����������ڼ�����ң�
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
    /// ���ݱ��ؿͻ���ID��ȡ��ID�ͻ��˵��������
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
        //������ɫID��ĳ������˰�ť�Ŀͻ��ˣ�����ɫ�Ƿ�ʹ����
        if (!IsColorAvailable(colorId))
        {
            return;
        }
        //����ɫδ��ʹ�ã���������ͻ��ˣ�����˰�ť�Ŀͻ��ˣ�����������е���ɫID��
        //��ȷ���͸�RPC�Ŀͻ��˵�����ǵڼ������
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        //�޷�ֱ�Ӹ����Զ���ṹ��ĳ�Ա��ֵ����netcode��ʾ�Ǳ������޷�ֱ���޸ģ�
        //playerDataNetworkList[playerDataIndex].clientId = colorId;

        //���������Ȼ�ȡ������ݵĽṹ��
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        
        //�޸ĳ�Ա
        playerData.colorId = colorId;

        //�����ĺ�Ľṹ�帲�ǵ��б���
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    /// <summary>
    /// ����Ƿ����������ʹ���˸���ɫ
    /// </summary>
    /// <param name="colorId"></param>
    /// <returns></returns>
    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)//�����������ӵ��������
        {
            if (playerData.colorId == colorId)//Ѱ����������������Ƿ�����ɫID������������ɫIDһ��
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
