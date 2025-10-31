using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 负责加载场景，新增多人游戏加载场景(使用这个才能自动生成网络物体)
/// </summary>
public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene
    }

    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        //存储目标场景
        Loader.targetScene = targetScene;//因为是静态类，这里不能用this.targetScene（通过实例访问）
        //先切换至加载场景
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
        //加载目标场景（这样是错误的：Unity的场景加载是"立即请求"但不是"立即完成"，连续请求会互相覆盖！）
        //SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    /// <summary>
    /// 切换至目标场景
    /// </summary>
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
