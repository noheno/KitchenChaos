using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 脚本化对象：食谱，包含了最终成品的所有食材
/// </summary>
[CreateAssetMenu]
public class RecipeSO : ScriptableObject
{
    /// <summary>
    /// 列表，包含该生成食谱的所有食材
    /// </summary>
    public List<KitchenObjectSO> kitchenObjectSOList;
    public string recipeName;
}
