using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        //存储目标场景
        Loader.targetScene = targetScene;//因为是静态类，这里不能用this.targetScene（通过实例访问）
        //先切换至加载场景，但是这样必须加载完加载场景才能到目标场景
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    /// <summary>
    /// 再切换至目标场景
    /// </summary>
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
