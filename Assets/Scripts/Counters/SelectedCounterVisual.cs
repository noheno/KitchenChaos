using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;
    private void Start()
    {
        Player.Instacne.OnSelectedcounterChanged += Player_OnSelectedcounterChanged;
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
