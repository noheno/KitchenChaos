using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������ɫ��ť����������øð�ť����ɫ
/// </summary>
public class CharacterColorSelectSingleUI : MonoBehaviour
{
    /// <summary>
    /// �ֶ����õ�ID�����ڱ��Ҫʹ�÷���������ɫ�б��еĵڼ�����ɫ
    /// </summary>
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;


    private void Awake()
    {
        //ÿ�ε����ť�޸ĵ���ð�ť�Ŀͻ��˵���ҵ���ɫ
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.ChangePlayerColor(colorId);//������ҵ���ɫ����Ϊ��ť��ɫ����
        });
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        image.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);//����ť�����Ұ�����ɫ�б��е�˳��������ɫ
        UpdateIsSelected();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();//ÿ�ε����ť���°�ťѡ����Ч
    }

    /// <summary>
    /// ���°�ťѡ����Ч
    /// </summary>
    private void UpdateIsSelected()
    {
        if (KitchenGameMultiplayer.Instance.GetPlayerData().colorId == colorId)//���ĳ���ͻ��˵���ɫID(�������ɫ)�͸���ɫ��ť��IDһ��
        {
            selectedGameObject.SetActive(true);//�ð�ť��ʾѡ����Ч
        }
        else
        {
            selectedGameObject.SetActive(false);//��������¸ð�ť������ʾ��Ч
        }
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }

}
