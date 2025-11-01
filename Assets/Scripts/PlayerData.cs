using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>,INetworkSerializable
{
    public ulong clientId;
    /// <summary>
    /// ϵͳ��ʹ����ɫ�б����ݸ�ID�趨��ͬ�������ɫ
    /// </summary>
    public int colorId;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && colorId == other.colorId;
    }

    /// <summary>
    /// ���л�clientId��colorId
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializer"></param>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
    }
}
