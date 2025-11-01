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
        OnAnyPlayerSpawned = null;//������м���
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

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);//��ȡÿ�����뵽��Ϸ��������ҵ��������
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));//����������ݡ���ȡ��ɫID->���ݸ���ɫID�趨�����ɫ
    }

    public override void OnNetworkSpawn()
    {
        //����ǲ��Ǳ�����ң�ÿ���ͻ���ӵ�е�ֻ��һ������ң�
        if (IsOwner)
        {
            LocalInstacne = this;
        }
        //transform.position = spawnPositonList[(int)OwnerClientId];//�����Ⱥ�����˳���ڲ�ͬ��λ���������
        transform.position = spawnPositonList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];//�����Ⱥ�����˳���ڲ�ͬ��λ���������
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
    /// ��F��ʱ�Ľ���
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
    /// ��Eʱ����ѡ�й�̨��Ϊ�գ�����н���
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
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);


        float moveDistance = moveSpeed * Time.deltaTime;//ÿ֡�ƶ��ľ���
        float playerRadius = .7f;
        //float playerHeight = 2f;
        canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisonsLayerMask);//ǰ��������Ϊ������������˵㣬����������Ϊ������İ뾶

        // ����޷��ƶ���������X��(ˮƽ)��Z�᷽��(��ֱ)�ƶ�
        if (!canMove)
        {
            // ����һ������X�᷽����ƶ�����
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            // �����X�᷽���Ƿ�����ƶ���x����û�������Ұѽ��Ҽ�����Ϊx����
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisonsLayerMask);

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
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisonsLayerMask);
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

    #region CapsuleCast���ӻ�
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

    #region BoxCast���ӻ�
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !IsOwner) return;

        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        if (moveDir == Vector3.zero) return;

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        Vector3 boxSize = Vector3.one * playerRadius * 2;

        // �����ײ
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir,
            Quaternion.identity, moveDistance, collisonsLayerMask);

        // ������ɫ
        Gizmos.color = canMove ? Color.green : Color.red;

        // ����������
        Gizmos.DrawWireCube(transform.position, boxSize);

        // �����յ����
        Vector3 endPos = transform.position + moveDir * moveDistance;
        Gizmos.DrawWireCube(endPos, boxSize);

        // ���Ƽ��·��
        Gizmos.DrawLine(transform.position, endPos);

        // �����ײ��������ײ��
        if (!canMove)
        {
            RaycastHit hit;
            if (Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, out hit,
                Quaternion.identity, moveDistance, collisonsLayerMask))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(hit.point, Vector3.one * 0.2f);
                Gizmos.DrawLine(transform.position, hit.point);

                // ������ײ�㴦�ķ���
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hit.point, hit.normal * 0.5f);
            }
        }

        // ���Ʒ������⣨X��Z����
        Gizmos.color = Color.cyan;

        // X�᷽����
        Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
        Vector3 endPosX = transform.position + moveDirX * moveDistance;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, endPosX + Vector3.up * 0.1f);

        // Z�᷽����  
        Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
        Vector3 endPosZ = transform.position + moveDirZ * moveDistance;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, endPosZ + Vector3.up * 0.2f);
    }
    #endregion
}
