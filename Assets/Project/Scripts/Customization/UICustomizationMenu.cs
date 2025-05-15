using System;
using System.Collections.Generic;
using Project.Scripts.Character;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Project.Scripts.Customization
{
    public class UICustomizationMenu: MonoBehaviour
    {
        [Title("Skin Item Configs")]
        [SerializeField] private SkinItemConfig[] _skinItemConfigs;
        [SerializeField] private PlayerCustomization _playerCustomization;
        [Title("Skin Item Colors")]
        [SerializeField] private SkinItemColorConfig[] _skinColors;
        [SerializeField] private SkinItemColorConfig[] _hairColors;
        [SerializeField] private SkinItemColorConfig[] _eyeColors;
        [Title("Buttons")]
        [SerializeField] private Button _nextTopButton;
        [SerializeField] private Button _prevTopButton;
        [SerializeField] private Button _nextBottomButton;
        [SerializeField] private Button _prevBottomButton;
        [SerializeField] private Button _nextShoesButton;
        [SerializeField] private Button _prevShoesButton;
        [SerializeField] private Button _nextHairButton;
        [SerializeField] private Button _prevHairButton;
        [SerializeField] private Button _nextBangsButton;
        [SerializeField] private Button _prevBangsButton;
        [SerializeField] private Button _nextGlassesButton;
        [SerializeField] private Button _prevGlassesButton;
        [SerializeField] private Button _randomizeButton;
        [Title("Color")] 
        [SerializeField] private Button _colorButtonPrefab;
        [SerializeField] private Transform _skinColorParent;
        [SerializeField] private Transform _hairColorParent;
        [SerializeField] private Transform _eyeColorParent;
        
        private readonly Dictionary<SkinItemType, List<SkinItemConfig>> _skinItems = new ();
        private readonly Dictionary<SkinItemType, int> _skinItemIndex = new ();
        private readonly List<Button> _colorButtons = new ();

        private SkinData _skinData;

        [Inject]
        private void Construct(SkinData skinData)
        {
            _skinData = skinData;
        }
        
        private void Awake()
        {
            _nextTopButton.onClick.AddListener(NextTop);
            _prevTopButton.onClick.AddListener(PrevTop);
            _nextBottomButton.onClick.AddListener(NextBottom);
            _prevBottomButton.onClick.AddListener(PrevBottom);
            _nextShoesButton.onClick.AddListener(NextShoes);
            _prevShoesButton.onClick.AddListener(PrevShoes);
            _nextHairButton.onClick.AddListener(NextHair);
            _prevHairButton.onClick.AddListener(PrevHair);
            _nextBangsButton.onClick.AddListener(NextBangs);
            _prevBangsButton.onClick.AddListener(PrevBangs);
            _nextGlassesButton.onClick.AddListener(NextGlasses);
            _prevGlassesButton.onClick.AddListener(PrevGlasses);
            _randomizeButton.onClick.AddListener(Randomize);
            foreach (var color in _hairColors)
            {
                var prefab = Instantiate(_colorButtonPrefab, _hairColorParent);
                var childImage = prefab.transform.GetChild(0).GetComponent<Image>();
                childImage.color = color.SkinItemColors[0].Color;
                prefab.onClick.AddListener(() =>
                {
                    _skinData.SetHairColor(color);
                    _playerCustomization.SetSkin(_skinData);
                });
                _colorButtons.Add(prefab);
            }
            var go = new GameObject(".");
            go.AddComponent<RectTransform>();
            go.transform.SetParent(_hairColorParent);
            foreach (var color in _eyeColors)
            {
                var prefab = Instantiate(_colorButtonPrefab, _eyeColorParent);
                var childImage = prefab.transform.GetChild(0).GetComponent<Image>();
                var eyeColor = color.SkinItemColors[0].Color;
                eyeColor.a = 1;
                childImage.color = eyeColor;
                prefab.onClick.AddListener(() =>
                {
                    _skinData.SetEyeColor(color);
                    _playerCustomization.SetSkin(_skinData);
                });
                _colorButtons.Add(prefab);
            }
            var go2 = new GameObject(".");
            go2.AddComponent<RectTransform>();
            go2.transform.SetParent(_eyeColorParent);
            foreach (var color in _skinColors)
            {
                var prefab = Instantiate(_colorButtonPrefab, _skinColorParent);
                var childImage = prefab.transform.GetChild(0).GetComponent<Image>();
                childImage.color = color.SkinItemColors[1].Color;
                prefab.onClick.AddListener(() =>
                {
                    _skinData.SetSkinColor(color);
                    _playerCustomization.SetSkin(_skinData);
                });
                _colorButtons.Add(prefab);
            }
            var go3 = new GameObject(".");
            go3.AddComponent<RectTransform>();
            go3.transform.SetParent(_skinColorParent);
        }

        private void Start()
        {
            foreach (SkinItemType skinItemType in Enum.GetValues(typeof(SkinItemType)))
            {
                _skinItems.Add(skinItemType, new List<SkinItemConfig>());
                _skinItemIndex.Add(skinItemType, 0);
            }
            foreach (var skinItemConfig in _skinItemConfigs)
                _skinItems[skinItemConfig.SkinItemType].Add(skinItemConfig);
            if(_skinData.GetSkinItem(SkinItemType.Top) == null)
            {
                _skinData.SetSkinItem(SkinItemType.Top, _skinItems[SkinItemType.Top][0]);
                _skinData.SetSkinItem(SkinItemType.Bottom, _skinItems[SkinItemType.Bottom][0]);
                _skinData.SetSkinItem(SkinItemType.Shoes, _skinItems[SkinItemType.Shoes][0]);
                _skinData.SetSkinItem(SkinItemType.Hair, _skinItems[SkinItemType.Hair][0]);
                _skinData.SetSkinItem(SkinItemType.Bangs, _skinItems[SkinItemType.Bangs][0]);
                _skinData.SetSkinItem(SkinItemType.Glasses, _skinItems[SkinItemType.Glasses][0]);
                _skinData.SetSkinColor(_skinColors[0]);
                _skinData.SetHairColor(_hairColors[0]);
                _skinData.SetEyeColor(_eyeColors[0]);
                _playerCustomization.SetSkin(_skinData);
            }
        }

        private void OnDestroy()
        {
            _nextTopButton.onClick.RemoveListener(NextTop);
            _nextBottomButton.onClick.RemoveListener(NextBottom);
            _nextShoesButton.onClick.RemoveListener(NextShoes);
            _nextHairButton.onClick.RemoveListener(NextHair);
            _nextBangsButton.onClick.RemoveListener(NextBangs);
            _nextGlassesButton.onClick.RemoveListener(NextGlasses);
            _randomizeButton.onClick.RemoveListener(Randomize);
            _prevTopButton.onClick.RemoveListener(PrevTop);
            _prevBottomButton.onClick.RemoveListener(PrevBottom);
            _prevShoesButton.onClick.RemoveListener(PrevShoes);
            _prevHairButton.onClick.RemoveListener(PrevHair);
            _prevBangsButton.onClick.RemoveListener(PrevBangs);
            _prevGlassesButton.onClick.RemoveListener(PrevGlasses);
            foreach (var btn in _colorButtons)
                btn.onClick.RemoveAllListeners();
        }

        private SkinItemConfig GetNextItem(SkinItemType skinItemType)
        {
            if (_skinItems[SkinItemType.Top].Count == 0)
            {
                Debug.LogError("No skin items of type Top");
                return null;
            }
            _skinItemIndex[skinItemType]++;
            if (_skinItemIndex[skinItemType] >= _skinItems[skinItemType].Count)
                _skinItemIndex[skinItemType] = 0;
            var skinItemConfig = _skinItems[skinItemType][_skinItemIndex[skinItemType]];
            if (skinItemConfig == null) return null;
            return skinItemConfig;
        }
        
        private SkinItemConfig GetPrevItem(SkinItemType skinItemType)
        {
            if (_skinItems[SkinItemType.Top].Count == 0)
            {
                Debug.LogError("No skin items of type Top");
                return null;
            }
            _skinItemIndex[skinItemType]--;
            if (_skinItemIndex[skinItemType] < 0)
                _skinItemIndex[skinItemType] = _skinItems[skinItemType].Count - 1;
            var skinItemConfig = _skinItems[skinItemType][_skinItemIndex[skinItemType]];
            if (skinItemConfig == null) return null;
            return skinItemConfig;
        }
        
        private void NextTop()
        {
            var skinItemConfig = GetNextItem(SkinItemType.Top);
            _skinData.SetSkinItem(SkinItemType.Top, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void PrevTop()
        {
            var skinItemConfig = GetPrevItem(SkinItemType.Top);
            _skinData.SetSkinItem(SkinItemType.Top, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void NextBottom()
        {
            var skinItemConfig = GetNextItem(SkinItemType.Bottom);
            _skinData.SetSkinItem(SkinItemType.Bottom, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void PrevBottom()
        {
            var skinItemConfig = GetPrevItem(SkinItemType.Bottom);
            _skinData.SetSkinItem(SkinItemType.Bottom, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void NextShoes()
        { 
            var skinItemConfig = GetNextItem(SkinItemType.Shoes);
            _skinData.SetSkinItem(SkinItemType.Shoes, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void PrevShoes()
        {
            var skinItemConfig = GetPrevItem(SkinItemType.Shoes);
            _skinData.SetSkinItem(SkinItemType.Shoes, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void NextHair()
        {
             var skinItemConfig = GetNextItem(SkinItemType.Hair);
            _skinData.SetSkinItem(SkinItemType.Hair, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void PrevHair()
        {
            var skinItemConfig = GetPrevItem(SkinItemType.Hair);
            _skinData.SetSkinItem(SkinItemType.Hair, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void NextBangs()
        {
            var skinItemConfig = GetNextItem(SkinItemType.Bangs);
            _skinData.SetSkinItem(SkinItemType.Bangs, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void PrevBangs()
        {
            var skinItemConfig = GetPrevItem(SkinItemType.Bangs);
            _skinData.SetSkinItem(SkinItemType.Bangs, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void NextGlasses()
        {
            var skinItemConfig = GetNextItem(SkinItemType.Glasses);
            _skinData.SetSkinItem(SkinItemType.Glasses, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void PrevGlasses()
        {
            var skinItemConfig = GetPrevItem(SkinItemType.Glasses);
            _skinData.SetSkinItem(SkinItemType.Glasses, skinItemConfig);
            _playerCustomization.SetSkin(_skinData);
        }

        private void Randomize()
        {
            foreach (var skinItemType in Enum.GetValues(typeof(SkinItemType)))
            {
                var skinItemConfig = _skinItems[(SkinItemType)skinItemType][UnityEngine.Random.Range(0, _skinItems[(SkinItemType)skinItemType].Count)];
                if (skinItemConfig == null) continue;
                _skinData.SetSkinItem((SkinItemType)skinItemType, skinItemConfig);
            }
            _skinData.SetSkinColor(_skinColors[UnityEngine.Random.Range(0, _skinColors.Length)]);
            _skinData.SetHairColor(_hairColors[UnityEngine.Random.Range(0, _hairColors.Length)]);
            _skinData.SetEyeColor(_eyeColors[UnityEngine.Random.Range(0, _eyeColors.Length)]);
            _playerCustomization.SetSkin(_skinData);
        }
    }
}