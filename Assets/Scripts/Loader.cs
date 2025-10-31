using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// ������س���������������Ϸ���س���(ʹ����������Զ�������������)
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
        //�洢Ŀ�곡��
        Loader.targetScene = targetScene;//��Ϊ�Ǿ�̬�࣬���ﲻ����this.targetScene��ͨ��ʵ�����ʣ�
        //���л������س���
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
        //����Ŀ�곡���������Ǵ���ģ�Unity�ĳ���������"��������"������"�������"����������ụ�า�ǣ���
        //SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    /// <summary>
    /// �л���Ŀ�곡��
    /// </summary>
    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
