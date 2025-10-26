using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 4;
    private int platesSpawnAmount;
    private int platesSpawnAmountMax = 4;

    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        spawnPlateTimer += Time.deltaTime;
        if (KitchenGameManager.Instance.IsGamePlaying() && spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0;
            if (platesSpawnAmount < platesSpawnAmountMax)
            {
                SpawnPlateServerRpc();//֪ͨ��������Ҫ��������
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();//�㲥�����пͻ�������������
    }
    /// <summary>
    /// �������������Լ������¼����������ɡ�
    /// </summary>
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        platesSpawnAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(Player player)
    {
        //�������û�ж���
        if (!player.HasKitchenObject())
        {
            if (platesSpawnAmount > 0)
            {
                //�����Ӹ����
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                //���������������
                InteractLogicServerRpc();//֪ͨ��������Ҫ��������
            }
        }
    }

    /// <summary>
    /// ��֪���������ڽ���
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();//�㲥�����пͻ��˰���������
    }

    /// <summary>
    /// �������������Լ������¼����������ߡ�
    /// </summary>
    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        platesSpawnAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }

}
