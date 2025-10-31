using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 加载完Loading界面后用于加载目标界面
/// </summary>
public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;
    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Loader.LoaderCallback();
        }
    }
}
