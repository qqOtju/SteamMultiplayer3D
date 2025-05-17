using System;
using System.Collections.Generic;

namespace Project.Scripts.Customization
{
    public class SkinData
    {
        private readonly Dictionary<SkinItemType, SkinItemConfig> _skinItems = new();

        public SkinData()
        {
            foreach (SkinItemType skinItemType in Enum.GetValues(typeof(SkinItemType)))
                _skinItems.Add(skinItemType, null);
        }

        public SkinItemColorConfig SkinColor { get; set; }
        public SkinItemColorConfig HairColor { get; set; }
        public SkinItemColorConfig EyeColor { get; set; }

        public void SetSkinItem(SkinItemType skinItemType, SkinItemConfig skinItemConfig)
        {
            if (_skinItems.ContainsKey(skinItemType))
                _skinItems[skinItemType] = skinItemConfig;
        }

        public SkinItemConfig GetSkinItem(SkinItemType skinItemType)
        {
            if (_skinItems.TryGetValue(skinItemType, out var item))
                return item;
            return null;
        }

        public void SetSkinColor(SkinItemColorConfig skinColor)
        {
            SkinColor = skinColor;
        }

        public void SetHairColor(SkinItemColorConfig hairColor)
        {
            HairColor = hairColor;
        }

        public void SetEyeColor(SkinItemColorConfig eyeColor)
        {
            EyeColor = eyeColor;
        }
    }
}