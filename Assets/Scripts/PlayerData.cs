using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>,INetworkSerializable
{
    public ulong clientId;
    /// <summary>
    /// 系统会使用颜色列表，根据该ID设定不同对象的颜色
    /// </summary>
    public int colorId;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && colorId == other.colorId;
    }

    /// <summary>
    /// 序列化clientId和colorId
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializer"></param>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
    }
}
