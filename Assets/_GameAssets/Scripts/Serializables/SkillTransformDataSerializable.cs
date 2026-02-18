using Unity.Netcode;
using UnityEngine;

public struct SkillTransformDataSerializables : INetworkSerializeByMemcpy
{
    public Vector3 Position;
    public Quaternion Rotation;
    public SkillType SkillType;
    public NetworkObject NetworkObject;

    public SkillTransformDataSerializables(Vector3 position,  Quaternion rotation, SkillType skillType,
        NetworkObject networkObject)
    {
        Position = position;
        Rotation = rotation;
        SkillType = skillType;
        NetworkObject = networkObject;
    }
}
