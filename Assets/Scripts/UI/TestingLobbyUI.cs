using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ������������UI�����Դ�����Ϸ(StartHost)�ͼ�����Ϸ(StartClient)
/// </summary>
public class TestingLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });
        joinGameButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
        });//�¿ͻ�������ʱ���Զ�ͬ������������ǰ���������ü��ֶ���LoadNetwork
    }
}
