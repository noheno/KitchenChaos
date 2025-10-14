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
            Debug.LogError("����һ�����ʵ����");
        }
        Instacne = this;
    }

    private void Start()
    {
        gameInput.OnInteractAciton += GameInput_OnInteractAciton;
        gameInput.OnInteractAlternateAciton += GameInput_OnInteractAlternateAciton;
    }

    /// <summary>
    /// ��F��ʱ�Ľ���
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
    /// ��Eʱ����ѡ�й�̨��Ϊ�գ�����н���
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
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, counterLayerMask))//��⡰��̨��
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))//��⵽��������ClearCounter���������ClearCounter�ű���
            {
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else//�����⵽������û��ClearCounter���������ClearCounter�ű���
            {
                SetSelectedCounter(null);
            }
        }
        else//û�м�⵽��̨
        {
            SetSelectedCounter(null);
        }
        //Debug.Log(selectedCounter);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;//���±�ѡ�еĹ�̨
        OnSelectedcounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;//ÿ֡�ƶ��ľ���
        float playerRadius = .7f;
        float playerHeight = 2f;
        canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);//ǰ��������Ϊ������������˵㣬����������Ϊ������İ뾶

        // ����޷��ƶ���������X��(ˮƽ)��Z�᷽��(��ֱ)�ƶ�
        if (!canMove)
        {
            // ����һ������X�᷽����ƶ�����
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            // �����X�᷽���Ƿ�����ƶ�
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                // ���������X���ƶ���������ƶ�����ΪX�᷽��
                moveDir = moveDirX;
            }
            else
            {
                // ����޷���X���ƶ���������Z�᷽���ƶ�
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                // �����Z�᷽���Ƿ�����ƶ�
                canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
                if (canMove)
                {
                    // ���������Z���ƶ���������ƶ�����ΪZ�᷽��
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
