using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Project.Scripts.Customization
{
    public class PlayerCustomization: MonoBehaviour
    {
        [Title("Body Parts")]
        [SerializeField] private Transform _lowerArms;
        [SerializeField] private Transform _upperArms;
        [SerializeField] private Transform _feet;
        [SerializeField] private Transform _hands;
        [SerializeField] private Transform _hips;
        [SerializeField] private Transform _legsKnee;
        [SerializeField] private Transform _legsLower;
        [SerializeField] private Transform _legsUpper;
        [SerializeField] private Transform _shoulders;
        [SerializeField] private Transform _torso;
        [SerializeField] private Transform _neck;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _hairParent;
        [SerializeField] private Transform _bangsParent;
        [Title("Materials")]
        [SerializeField] private SkinnedMeshRenderer _headMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer[] _bodyMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer[] _hairMeshRenderer;
        [Title("Skin Items")]
        [SerializeField] private Transform[] _tops;
        [SerializeField] private Transform[] _bottoms;
        [SerializeField] private Transform[] _shoes;
        [SerializeField] private Transform[] _hairs;
        [SerializeField] private Transform[] _bangs;
        [SerializeField] private Transform[] _glasses;

        private Material[] _skinMaterials;
        private Material[] _hairMaterials;
        private Material _eyeMaterial;
        private SkinData _skinData;

        [Inject]
        private void Construct(SkinData skinData)
        {
            _skinData = skinData;
        }
        
        private void Awake()
        {
            _skinMaterials = new Material[_bodyMeshRenderer.Length + 1];
            _skinMaterials[0] = _headMeshRenderer.material;
            for (int i = 0; i < _bodyMeshRenderer.Length; i++)
                _skinMaterials[i + 1] = _bodyMeshRenderer[i].material;
            _hairMaterials = new Material[_hairMeshRenderer.Length];
            for (int i = 0; i < _hairMeshRenderer.Length; i++)
                _hairMaterials[i] = _hairMeshRenderer[i].material;
            _eyeMaterial = _headMeshRenderer.materials[2];
        }

        private void Start()
        {
            if (_skinData == null || 
                _skinData.GetSkinItem(SkinItemType.Top) == null)
                return;
            SetSkin(_skinData);
        }

        public void SetSkin(SkinData skinData)
        {
            if (skinData == null) return;
            _lowerArms.gameObject.SetActive(true);
            _upperArms.gameObject.SetActive(true);
            _feet.gameObject.SetActive(true);
            _hands.gameObject.SetActive(true);
            _hips.gameObject.SetActive(true);
            _legsKnee.gameObject.SetActive(true);
            _legsLower.gameObject.SetActive(true);
            _legsUpper.gameObject.SetActive(true);
            _shoulders.gameObject.SetActive(true);
            _torso.gameObject.SetActive(true);
            _neck.gameObject.SetActive(true);
            _head.gameObject.SetActive(true);
            _hairParent.gameObject.SetActive(true);
            _bangsParent.gameObject.SetActive(true);
            SetSkinColor(skinData.SkinColor);
            SetHairColor(skinData.HairColor);
            SetEyeColor(skinData.EyeColor);
            foreach (var skinItemType in Enum.GetValues(typeof(SkinItemType)))
            {
                var skinItemConfig = skinData.GetSkinItem((SkinItemType)skinItemType);
                if (skinItemConfig == null) continue;
                SetBodyParts(skinItemConfig.BodyPart);
                switch (skinItemConfig.SkinItemType)
                {
                    case SkinItemType.Top:
                        SetTop(skinItemConfig.ItemName);
                        break;
                    case SkinItemType.Bottom:
                        SetBottom(skinItemConfig.ItemName);
                        break;
                    case SkinItemType.Shoes:
                        SetShoes(skinItemConfig.ItemName);
                        break;
                    case SkinItemType.Hair:
                        SetHair(skinItemConfig.ItemName);
                        break;
                    case SkinItemType.Bangs:
                        SetBangs(skinItemConfig.ItemName);
                        break;
                    case SkinItemType.Glasses:
                        SetGlasses(skinItemConfig.ItemName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SetBodyParts(BodyPart bodyParts)
        {
            if ((bodyParts & BodyPart.LowerArms) == BodyPart.LowerArms)
                _lowerArms.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.UpperArms) == BodyPart.UpperArms)
                _upperArms.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Feet) == BodyPart.Feet)
                _feet.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Hands) == BodyPart.Hands)
                _hands.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Hips) == BodyPart.Hips)
                _hips.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.LegsKnee) == BodyPart.LegsKnee)
                _legsKnee.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.LegsLower) == BodyPart.LegsLower)
                _legsLower.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.LegsUpper) == BodyPart.LegsUpper)
                _legsUpper.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Shoulders) == BodyPart.Shoulders)
                _shoulders.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Torso) == BodyPart.Torso)
                _torso.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Neck) == BodyPart.Neck)
                _neck.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Head) == BodyPart.Head)
                _head.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Hair) == BodyPart.Hair)
                _hairParent.gameObject.SetActive(false);
            if ((bodyParts & BodyPart.Bangs) == BodyPart.Bangs)
                _bangsParent.gameObject.SetActive(false);
        }

        private void SetTop(string itemName)
        {
            foreach (var top in _tops)
                top.gameObject.SetActive(top.name == itemName);
        }

        private void SetBottom(string itemName)
        {
            foreach (var bottom in _bottoms)
                bottom.gameObject.SetActive(bottom.name == itemName);
        }

        private void SetShoes(string itemName)
        {
            foreach (var shoes in _shoes)
                shoes.gameObject.SetActive(shoes.name == itemName);
        }

        private void SetHair(string itemName)
        {
            foreach (var hair in _hairs)
                hair.gameObject.SetActive(hair.name == itemName);
        }

        private void SetBangs(string itemName)
        {
            foreach (var bangs in _bangs)
                bangs.gameObject.SetActive(bangs.name == itemName);
        }

        private void SetGlasses(string itemName)
        {
            foreach (var glasses in _glasses)
                glasses.gameObject.SetActive(glasses.name == itemName);
        }

        private void SetSkinColor(SkinItemColorConfig skinItemColorConfig)
        {
            foreach (var color in skinItemColorConfig.SkinItemColors)
                foreach (var material in _skinMaterials)
                    material.SetColor(color.MaterialColorName, color.Color);
        }
        
        private void SetHairColor(SkinItemColorConfig skinItemColorConfig)
        {
            foreach (var color in skinItemColorConfig.SkinItemColors)
                foreach (var material in _hairMaterials)
                    material.SetColor(color.MaterialColorName, color.Color);
        }
        
        private void SetEyeColor(SkinItemColorConfig skinItemColorConfig)
        {
            foreach (var color in skinItemColorConfig.SkinItemColors)
                _eyeMaterial.SetColor(color.MaterialColorName, color.Color);
        }
    }
}