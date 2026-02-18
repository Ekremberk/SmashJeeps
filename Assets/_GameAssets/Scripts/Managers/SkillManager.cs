using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillManager : NetworkBehaviour
{
    public static SkillManager Instance {  get; private set; }

    public event Action OnMineCountReduced;

    [SerializeField] private MysteryBoxSkillsSO[] _mysteryBoxSkills;
    [SerializeField] private LayerMask _groundLayer;

    private Dictionary<SkillType, MysteryBoxSkillsSO> _skillsDictionary;

    private void Awake()
    {
        Instance = this;

        _skillsDictionary = new Dictionary<SkillType, MysteryBoxSkillsSO>();

        foreach (MysteryBoxSkillsSO skill in _mysteryBoxSkills)
        {
            _skillsDictionary[skill.SkillType] = skill;
        }
    }

    public void ActivateSkill(SkillType skillType, Transform playerTransform, ulong spawnerClientId)
    {
        SkillTransformDataSerializables skillTransformData
            = new SkillTransformDataSerializables(playerTransform.position, playerTransform.rotation,
            skillType, playerTransform.GetComponent<NetworkObject>());

        if (!IsServer)
        {
            RequestSpawnRpc(skillTransformData, spawnerClientId);
            return;
        }

        SpawnSkill(skillTransformData, spawnerClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RequestSpawnRpc(SkillTransformDataSerializables skillTransformDataSerializables,
        ulong spawnerClientId)
    {
        SpawnSkill(skillTransformDataSerializables, spawnerClientId);
    }

    private async void SpawnSkill(SkillTransformDataSerializables skillTransformDataSerializables,
        ulong spawnerClientId)
    {
        if(!_skillsDictionary.TryGetValue(skillTransformDataSerializables.SkillType, out MysteryBoxSkillsSO skillData))
        {
            Debug.LogError($"Spawn Skill:  {skillTransformDataSerializables.SkillType} not found!");
            return;
        }

        if (skillTransformDataSerializables.SkillType == SkillType.Mine)
        {
            Vector3 spawnPosition = skillTransformDataSerializables.Position;
            Vector3 spawnDirection = skillTransformDataSerializables.Rotation * Vector3.forward;

            for (int i = 0; i < skillData.SkillData.SpawnAmountOrTimer; i++)
            {
                Vector3 offset = spawnDirection * (i * 3f);
                skillTransformDataSerializables.Position = spawnPosition + offset;

                Spawn(skillTransformDataSerializables, spawnerClientId, skillData);
                await UniTask.Delay(200);
                OnMineCountReduced?.Invoke();
            }
        }
        else
        {
            Spawn(skillTransformDataSerializables, spawnerClientId, skillData);
        }
    }

    private void Spawn(SkillTransformDataSerializables skillTransformDataSerializables,
        ulong spawnerClientId, MysteryBoxSkillsSO skillData)
    {
        if (IsServer)
        {
            Transform skillInstance = Instantiate(skillData.SkillData.SkillPrefab);
            skillInstance.SetPositionAndRotation(skillTransformDataSerializables.Position, skillTransformDataSerializables.Rotation);
            var networkObject = skillInstance.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(spawnerClientId);

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(spawnerClientId, out var client))
            {
                if (skillData.SkillType != SkillType.Rocket)
                {
                    networkObject.TrySetParent(client.PlayerObject);
                }
                else
                {
                    PlayerSkillController playerSkillController = client.PlayerObject.GetComponent<PlayerSkillController>();
                    networkObject.transform.localPosition = playerSkillController.GetRocketLaunchPosition();
                    return;
                }

                if (skillData.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.transform.localPosition = Vector3.zero;
                }

                PositionDataSerializable positionDataSerializable = new PositionDataSerializable(
                    skillInstance.transform.localPosition + skillData.SkillData.SkillOffset
                );
                UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable, false);

                if (!skillData.SkillData.ShouldBeAttachedToParent)
                {
                    networkObject.TryRemoveParent();
                }

                if(skillData.SkillType == SkillType.FakeBox)
                {
                    float groundHeight = GetGroundHeight(skillData, skillInstance.position);

                    positionDataSerializable = new PositionDataSerializable(new Vector3(
                        skillInstance.transform.position.x,
                        groundHeight,
                        skillInstance.transform.position.z));

                    UpdateSkillPositionRpc(networkObject.NetworkObjectId, positionDataSerializable, true);
                }
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateSkillPositionRpc(ulong objectId, PositionDataSerializable positionDataSerializable,
        bool isSpecialPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObject))
        {
            if (isSpecialPosition)
            {
                // FAKE BOX SPECIAL
                networkObject.transform.position = positionDataSerializable.Position;
            }
            else
            {
                networkObject.transform.localPosition = positionDataSerializable.Position;
            }
        }
    }

    private float GetGroundHeight(MysteryBoxSkillsSO skillData, Vector3 position)
    {
        if (Physics.Raycast(new Vector3(position.x, position.y, position.z), Vector3.down,
            out RaycastHit hit, 10f, _groundLayer))
        {
            return skillData.SkillData.SkillOffset.y;
        }

        return skillData.SkillData.SkillOffset.y;
    }
}
