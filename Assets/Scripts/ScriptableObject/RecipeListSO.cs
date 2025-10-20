using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 脚本化对象：列表，包含所有食谱
/// </summary>
//[CreateAssetMenu]
public class RecipeListSO : ScriptableObject
{
    public List<RecipeSO> recipeSOList;
}
