using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;
    private void Start()
    {
        //����б��ص���
        if (Player.LocalInstacne != null)
        {
            Player.LocalInstacne.OnSelectedcounterChanged += Player_OnSelectedcounterChanged;
        }
        //û�б��ص������������ص���ʱ��
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (Player.LocalInstacne != null)
        {
            Player.LocalInstacne.OnSelectedcounterChanged -= Player_OnSelectedcounterChanged;//��
            Player.LocalInstacne.OnSelectedcounterChanged += Player_OnSelectedcounterChanged;
        }
    }

    private void Player_OnSelectedcounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (var item in visualGameObjectArray)
        {
            item.SetActive(true);
        }
    }
    private void Hide()
    {
        foreach (var item in visualGameObjectArray)
        {
            item.SetActive(false);
        }
    }
}
