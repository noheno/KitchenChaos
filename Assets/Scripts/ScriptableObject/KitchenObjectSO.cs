using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �ű���������Ҫ�е��ֱ�ʳ�����͵����ã�������unity���������ơ�ͼ���Ԥ����
/// </summary>
[CreateAssetMenu(/*fileName = "new kitchen object", menuName = "KitchenObjects"*/)]
public class KitchenObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
}
