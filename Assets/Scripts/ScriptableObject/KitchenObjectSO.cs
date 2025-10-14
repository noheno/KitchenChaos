using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 脚本化对象，主要承担分辨食材类型的作用，可以在unity中设置名称、图标和预制体
/// </summary>
[CreateAssetMenu(/*fileName = "new kitchen object", menuName = "KitchenObjects"*/)]
public class KitchenObjectSO : ScriptableObject
{
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
}
