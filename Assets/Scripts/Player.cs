using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPlayerPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;//清除所有监听
    }

    public static Player LocalInstacne { get; private set; }

    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedcounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisonsLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> spawnPositonList;
    [SerializeField] private PlayerVisual playerVisual;

    private bool isWalking;
    private bool canMove;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    #region State

    #endregion


    private void Start()
    {
        GameInput.Instance.OnInteractAciton += GameInput_OnInteractAciton;
        GameInput.Instance.OnInteractAlternateAciton += GameInput_OnInteractAlternateAciton;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);//获取每个加入到游戏场景的玩家的玩家数据
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));//根据玩家数据↑获取颜色ID->根据该颜色ID设定玩家颜色
    }

    public override void OnNetworkSpawn()
    {
        //检查是不是本地玩家（每个客户端拥有的只有一个的玩家）
        if (IsOwner)
        {
            LocalInstacne = this;
        }
        //transform.position = spawnPositonList[(int)OwnerClientId];//根据先后连接顺序在不同的位置生成玩家
        transform.position = spawnPositonList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];//根据先后连接顺序在不同的位置生成玩家
        OnAnyPlayerSpawned?.Invoke(this,EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    /// <summary>
    /// 按F键时的交互
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void GameInput_OnInteractAlternateAciton(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    /// <summary>
    /// 按E时若被选中柜台不为空，则进行交互
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameInput_OnInteractAciton(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        HandleMovement();
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        float interactDistance = 2f;
        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, counterLayerMask))//检测“柜台”
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))//检测到的物体有ClearCounter组件（挂载ClearCounter脚本）
            {
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else//如果检测到的物体没有ClearCounter组件（挂载ClearCounter脚本）
            {
                SetSelectedCounter(null);
            }
        }
        else//没有检测到柜台
        {
            SetSelectedCounter(null);
        }
        //Debug.Log(selectedCounter);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;//更新被选中的柜台
        OnSelectedcounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;//每帧移动的距离
        float playerRadius = .7f;
        //float playerHeight = 2f;
        canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisonsLayerMask);//前两个参数为胶囊体的两个端点，第三个参数为胶囊体的半径

        // 如果无法移动，则尝试沿X轴(水平)或Z轴方向(垂直)移动
        if (!canMove)
        {
            // 创建一个仅沿X轴方向的移动向量
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            // 检查沿X轴方向是否可以移动（x方向没有输入且把胶囊检测更换为x方向）
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisonsLayerMask);

            if (canMove)
            {
                // 如果可以沿X轴移动，则更新移动方向为X轴方向
                moveDir = moveDirX;
            }
            else
            {
                // 如果无法沿X轴移动，则尝试沿Z轴方向移动
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                // 检查沿Z轴方向是否可以移动
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisonsLayerMask);
                if (canMove)
                {
                    // 如果可以沿Z轴移动，则更新移动方向为Z轴方向
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDir * moveDistance;
        }

        float rotateSpeed = 10;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        isWalking = moveDir != Vector3.zero;
    }

    public bool IsWalking() => isWalking;


    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerPickedSomething?.Invoke(this,EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject() { return kitchenObject; }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject() => kitchenObject != null;

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }

    #region CapsuleCast可视化
    //private void OnDrawGizmos()
    //{
    //    if (!Application.isPlaying) return;

    //    Vector3 start = transform.position;
    //    Vector3 end = transform.position + Vector3.up * 2f; // playerHeight
    //    float radius = 0.7f; // playerRadius
    //    Vector3 direction = GameInput.Instance.GetMovementVectorNormalized();
    //    float distance = moveSpeed * Time.deltaTime;

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(start, radius);
    //    Gizmos.DrawWireSphere(end, radius);
    //    Gizmos.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
    //    Gizmos.DrawLine(start - Vector3.right * radius, end - Vector3.right * radius);
    //    Gizmos.DrawLine(start + Vector3.forward * radius, end + Vector3.forward * radius);
    //    Gizmos.DrawLine(start - Vector3.forward * radius, end - Vector3.forward * radius);

    //    Gizmos.color = Color.green;
    //    Gizmos.DrawRay(start, direction * distance);
    //}
    #endregion

    #region BoxCast可视化
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !IsOwner) return;

        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        if (moveDir == Vector3.zero) return;

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        Vector3 boxSize = Vector3.one * playerRadius * 2;

        // 检测碰撞
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir,
            Quaternion.identity, moveDistance, collisonsLayerMask);

        // 设置颜色
        Gizmos.color = canMove ? Color.green : Color.red;

        // 绘制起点盒子
        Gizmos.DrawWireCube(transform.position, boxSize);

        // 绘制终点盒子
        Vector3 endPos = transform.position + moveDir * moveDistance;
        Gizmos.DrawWireCube(endPos, boxSize);

        // 绘制检测路径
        Gizmos.DrawLine(transform.position, endPos);

        // 如果碰撞，绘制碰撞点
        if (!canMove)
        {
            RaycastHit hit;
            if (Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, out hit,
                Quaternion.identity, moveDistance, collisonsLayerMask))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(hit.point, Vector3.one * 0.2f);
                Gizmos.DrawLine(transform.position, hit.point);

                // 绘制碰撞点处的法线
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
            }
        }

        // 绘制分离轴检测（X和Z方向）
        Gizmos.color = Color.cyan;

        // X轴方向检测
        Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
        Vector3 endPosX = transform.position + moveDirX * moveDistance;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, endPosX + Vector3.up * 0.1f);

        // Z轴方向检测  
        Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
        Vector3 endPosZ = transform.position + moveDirZ * moveDistance;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, endPosZ + Vector3.up * 0.2f);
    }
    #endregion
}
