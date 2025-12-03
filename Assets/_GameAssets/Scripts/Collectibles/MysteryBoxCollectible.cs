using System.Xml.Serialization;
using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _collider;
    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect()
    {
        Debug.Log("Box Working");
        CollectRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(Respawn), _respawnTimer);
    }

    private void AnimateCollection()
    {
        _collider.enabled = false;
        _boxAnimator.SetTrigger(Const.BoxAnimations.IS_COLLECTED);
    }

    private void Respawn()
    {
        _boxAnimator.SetTrigger(Const.BoxAnimations.IS_RESPAWNED);
        _collider.enabled = true;
    }
}
