using System;
using UnityEngine;

namespace Project.Scripts.Customization
{
    [CreateAssetMenu(fileName = "SkinItemConfig", menuName = "Customization/SkinItemConfig")]
    public class SkinItemConfig: ScriptableObject
    {
        [SerializeField] private string _itemName;
        [SerializeField] private SkinItemType _skinItemType;
        [SerializeField] private BodyPart _bodyPart;
        
        public string ItemName => _itemName;
        public SkinItemType SkinItemType => _skinItemType;
        public BodyPart BodyPart => _bodyPart;
    }

    [Flags]
    public enum BodyPart
    {
        LowerArms = 1 << 0,
        UpperArms = 1 << 1,
        Feet = 1 << 2,
        Hands = 1 << 3,
        Hips = 1 << 4,
        LegsKnee = 1 << 5,
        LegsLower = 1 << 6,
        LegsUpper = 1 << 7,
        Shoulders = 1 << 8,
        Torso = 1 << 9,
        Neck = 1 << 10,
        Head = 1 << 11,
        Hair = 1 << 12,
        Bangs = 1 << 13,
        Glasses = 1 << 14,
        None = 0
    }

    public enum SkinItemType
    {
        Top,
        Bottom,
        Shoes,
        Hair,
        Bangs,
        Glasses
    }
}