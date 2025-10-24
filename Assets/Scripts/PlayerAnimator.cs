using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private Animator anim;
    [SerializeField] private Player player;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        anim.SetBool(IS_WALKING, player.IsWalking());
    }
}
