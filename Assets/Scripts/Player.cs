using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    public static Player Instacne { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedcounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private bool isWalking;
    private bool canMove;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    #region State

    #endregion

    private void Awake()
    {
        if (Instacne != null)
        {
            Debug.LogError("超过一个玩家实例！");
        }
        Instacne = this;
    }

    private void Start()
    {
        gameInput.OnInteractAciton += GameInput_OnInteractAciton;
        gameInput.OnInteractAlternateAciton += GameInput_OnInteractAlternateAciton;
    }

    /// <summary>
    /// 按F键时的交互
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void GameInput_OnInteractAlternateAciton(object sender, EventArgs e)
    {
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
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
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
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;//每帧移动的距离
        float playerRadius = .7f;
        float playerHeight = 2f;
        canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);//前两个参数为胶囊体的两个端点，第三个参数为胶囊体的半径

        // 如果无法移动，则尝试沿X轴(水平)或Z轴方向(垂直)移动
        if (!canMove)
        {
            // 创建一个仅沿X轴方向的移动向量
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            // 检查沿X轴方向是否可以移动
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

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
                canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.up * 2f; // playerHeight
        float radius = 0.7f; // playerRadius
        Vector3 direction = gameInput.GetMovementVectorNormalized();
        float distance = moveSpeed * Time.deltaTime;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);
        Gizmos.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
        Gizmos.DrawLine(start - Vector3.right * radius, end - Vector3.right * radius);
        Gizmos.DrawLine(start + Vector3.forward * radius, end + Vector3.forward * radius);
        Gizmos.DrawLine(start - Vector3.forward * radius, end - Vector3.forward * radius);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(start, direction * distance);
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject() { return kitchenObject; }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject() => kitchenObject != null;
}
