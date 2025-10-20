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
        //�洢Ŀ�곡��
        Loader.targetScene = targetScene;//��Ϊ�Ǿ�̬�࣬���ﲻ����this.targetScene��ͨ��ʵ�����ʣ�
        //���л������س������������������������س������ܵ�Ŀ�곡��
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    /// <summary>
    /// ���л���Ŀ�곡��
    /// </summary>
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
