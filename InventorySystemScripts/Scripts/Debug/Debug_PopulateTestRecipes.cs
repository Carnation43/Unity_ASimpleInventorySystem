using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 【测试脚本】
/// 仿照 Debug_GiveItems，
/// 这个脚本会在游戏开始时，强行向 RecipeBookManager 
/// 中填充测试配方，直到达到所需的数量，用于测试 UI 布局和滚动。
/// </summary>
public class Debug_PopulateTestRecipes : MonoBehaviour
{
    [Header("测试数据模板")]
    [Tooltip("拖入任何一个已创建的 Recipe 资产文件作为模板")]
    [SerializeField] private Recipe recipeTemplate;

    [Header("测试设置")]
    [Tooltip("你希望在配方书中看到的测试配方总数")]
    [SerializeField] private int desiredRecipeCount = 10;

    void Start()
    {
        if (recipeTemplate == null)
        {
            Debug.LogError("[Debug_PopulateTestRecipes] 失败：你必须在 Inspector 中拖入一个 'Recipe Template'！");
            return;
        }

        if (RecipeBookManager.instance == null)
        {
            Debug.LogError("[Debug_PopulateTestRecipes] 失败：RecipeBookManager.instance 为空。请确保此脚本在 RecipeBookManager 之后运行。");
            return;
        }

        // 1. 获取 RecipeBookManager 已经从 CraftingManager 加载的真实配方列表
        List<RecipeStatus> recipeList = RecipeBookManager.instance.AcquiredRecipes;
        int currentCount = recipeList.Count;

        int recipesToAdd = desiredRecipeCount - currentCount;

        if (recipesToAdd <= 0)
        {
            Debug.Log($"[Debug_PopulateTestRecipes] RecipeBookManager 中已有 {currentCount} 个配方，无需添加测试数据。");
            return;
        }

        // 2. 使用模板配方，创建新的 RecipeStatus，直到达到数量
        for (int i = 0; i < recipesToAdd; i++)
        {
            // 我们重复添加同一个配方资产，这对于测试 UI 来说没有问题
            // 它们会被包装在 10 个不同的 'RecipeStatus' 实例中
            RecipeStatus testStatus = new RecipeStatus(recipeTemplate);

            // [可选] 让测试数据看起来不一样
            // 比如，我们可以让一半的测试数据是“新”的，一半是“已解锁”的
            testStatus.isNew = (i % 2 == 0);
            testStatus.isUnlocked = (i % 3 == 0); // 让 1/3 的测试配方解锁

            recipeList.Add(testStatus);
        }

        Debug.Log($"[Debug_PopulateTestRecipes] 成功生成 {recipesToAdd} 个测试配方。总数: {recipeList.Count}");
    }
}