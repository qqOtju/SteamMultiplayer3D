using System;
using System.Collections.Generic;
using Project.Scripts.Customization;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Project.Scripts.Infrastructure
{
    public class ProjectInstaller: MonoInstaller
    {
        [Title("Skin Item Configs")]
        [SerializeField] private SkinItemConfig[] _skinItemConfigs;
        [SerializeField] private SkinItemColorConfig[] _skinColors;
        [SerializeField] private SkinItemColorConfig[] _hairColors;
        [SerializeField] private SkinItemColorConfig[] _eyeColors;
        
        private readonly Dictionary<SkinItemType, List<SkinItemConfig>> _skinItems = new ();

        public override void Start()
        {
            base.Start();
            //ToDo: Move this to a better place
            Application.targetFrameRate = 120;
        }

        public override void InstallBindings()
        {
            BindDiContainer();
            BindSkinData();
        }

        private void BindDiContainer()
        {
            Container.Bind<DiContainer>().FromInstance(Container).AsSingle();
        }

        private void BindSkinData()
        {
            foreach (SkinItemType skinItemType in Enum.GetValues(typeof(SkinItemType)))
                _skinItems.Add(skinItemType, new List<SkinItemConfig>());
            foreach (var skinItemConfig in _skinItemConfigs)
                _skinItems[skinItemConfig.SkinItemType].Add(skinItemConfig);
            var skinData = new SkinData();
            Randomize(skinData);
            Container.Bind<SkinData>().FromInstance(skinData).AsSingle();
        }

        private void Randomize(SkinData skinData)
        {
            foreach (var skinItemType in Enum.GetValues(typeof(SkinItemType)))
            {
                var skinItemConfig = _skinItems[(SkinItemType)skinItemType][UnityEngine.Random.Range(0, _skinItems[(SkinItemType)skinItemType].Count)];
                if (skinItemConfig == null) continue;
                skinData.SetSkinItem((SkinItemType)skinItemType, skinItemConfig);
            }
            skinData.SetSkinColor(_skinColors[UnityEngine.Random.Range(0, _skinColors.Length)]);
            skinData.SetHairColor(_hairColors[UnityEngine.Random.Range(0, _hairColors.Length)]);
            skinData.SetEyeColor(_eyeColors[UnityEngine.Random.Range(0, _eyeColors.Length)]);
        }
    }
}