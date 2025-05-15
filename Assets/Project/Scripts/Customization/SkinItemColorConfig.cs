using System;
using UnityEngine;

namespace Project.Scripts.Customization
{
    [CreateAssetMenu(fileName = "SkinItemColorConfig", menuName = "Customization/SkinItemColorConfig")]
    public class SkinItemColorConfig: ScriptableObject
    {
        [SerializeField] private SkinItemColor[] _skinItemColors;
        
        public SkinItemColor[] SkinItemColors => _skinItemColors;
    }
    
    [Serializable]
    public struct SkinItemColor
    {
        [SerializeField] private string _materialColorName;
        [SerializeField] private Color _color;
        
        public string MaterialColorName => _materialColorName;
        public Color Color => _color;
    }
}