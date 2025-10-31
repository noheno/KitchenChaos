using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������Loading��������ڼ���Ŀ�����
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
