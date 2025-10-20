using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurningFlashBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";
    private Animator animator;
    [SerializeField] private StoveCounter stoveCounter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        animator.SetBool(IS_FLASHING, false);
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;
        animator.SetBool(IS_FLASHING,true);
    }
}
