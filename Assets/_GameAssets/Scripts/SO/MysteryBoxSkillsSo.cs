using UnityEngine;

[CreateAssetMenu(fileName = "Mystery Box Skills", menuName = "Scriptable Objects/Mystery Box Skills")]

public class MysteryBoxSkillsSO : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private Sprite _skillIcon;
    [SerializeField] private SkillType _skillType;
    [SerializeField] private SkillUsageType _skillUsageType;
    [SerializeField] private SkillDataSo _skillData;
    public string SkillName => _skillName;
    public Sprite SkillIcon => _skillIcon;
    public SkillType SkillType => _skillType;
    public SkillUsageType SkillUsageType => _skillUsageType;
    public SkillDataSo SkillData => _skillData;
}
