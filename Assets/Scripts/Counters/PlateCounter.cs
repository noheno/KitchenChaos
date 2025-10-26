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
                SpawnPlateServerRpc();//通知服务器需要生成盘子
            }
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();//广播给所有客户端生成了盘子
    }
    /// <summary>
    /// 盘子数量增加以及触发事件“盘子生成”
    /// </summary>
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        platesSpawnAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(Player player)
    {
        //玩家手上没有东西
        if (!player.HasKitchenObject())
        {
            if (platesSpawnAmount > 0)
            {
                //把盘子给玩家
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                //拿走最上面的盘子
                InteractLogicServerRpc();//通知服务器需要拿走盘子
            }
        }
    }

    /// <summary>
    /// 告知服务器正在交互
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();//广播给所有客户端把盘子拿走
    }

    /// <summary>
    /// 盘子数量减少以及触发事件“盘子拿走”
    /// </summary>
    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        platesSpawnAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }

}
