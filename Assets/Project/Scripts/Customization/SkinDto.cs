using System;
using UnityEngine;

namespace Project.Scripts.Customization
{
    [Serializable]
    public struct SkinDto
    {
        public Color skinColor;
        public Color hairColorA1, hairColorA2;
        public Color eyeColorB1, eyeColorB2, eyeColorC1;
        public string top, bottom, shoes, hair, bangs, glasses;
        public BodyPart disabledParts;
    }
}