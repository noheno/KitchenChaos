using System;
using System.Collections;
using System.Collections.Generic;
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
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax)
        {
            spawnPlateTimer = 0;
            if (platesSpawnAmount < platesSpawnAmountMax)
            {
                platesSpawnAmount++;
                OnPlateSpawned?.Invoke(this,EventArgs.Empty);
            }
        }
    }

    public override void Interact(Player player)
    {
        //玩家手上没有东西
        if (!player.HasKitchenObject())
        {
            if (platesSpawnAmount > 0)
            {
                platesSpawnAmount--;
                //把盘子给玩家
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                //拿走最上面的盘子
                OnPlateRemoved?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
