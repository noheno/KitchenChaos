using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ű�������ʳ�ף����������ճ�Ʒ������ʳ��
/// </summary>
[CreateAssetMenu]
public class RecipeSO : ScriptableObject
{
    /// <summary>
    /// �б�����������ʳ�׵�����ʳ��
    /// </summary>
    public List<KitchenObjectSO> kitchenObjectSOList;
    public string recipeName;
}
