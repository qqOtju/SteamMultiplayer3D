using Mirror;
using Project.Scripts.UI.MainMenu;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Project.Scripts.Customization
{
    public class NetworkCustomizationPlayer: NetworkBehaviour
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
        
        private static readonly int ColorB1 = Shader.PropertyToID("_Color_B_1");
        private static readonly int ColorB2 = Shader.PropertyToID("_Color_B_2");
        private static readonly int ColorC1 = Shader.PropertyToID("_Color_C_1");
        private static readonly int ColorA1 = Shader.PropertyToID("_Color_A_1");
        private static readonly int ColorA2 = Shader.PropertyToID("_Color_A_2");
        
        private Material[] _skinMaterials;
        private Material[] _hairMaterials;
        private Material _eyeMaterial;
        
        [SyncVar(hook = nameof(OnSkinDTOChanged))]
        private string _jsonSkinDTO;
        
        private SkinData _skinData;
        private SkinDto _skinDTO;

        [Inject]
        private void Construct(SkinData skinData)
        {
            _skinData = skinData;
            Debug.Log($"Constructed: {skinData.GetSkinItem(SkinItemType.Top).ItemName}, {skinData.GetSkinItem(SkinItemType.Bottom).ItemName}, {skinData.GetSkinItem(SkinItemType.Shoes).ItemName}, {skinData.GetSkinItem(SkinItemType.Hair).ItemName}, {skinData.GetSkinItem(SkinItemType.Bangs).ItemName}, {skinData.GetSkinItem(SkinItemType.Glasses).ItemName}");
            //ClientSetSkin called here because Construct is called after OnStartAuthority
            if(!isOwned) return;
            ClientSetSkin(UILobbyPlayer.ConvertToDto(_skinData));
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

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isOwned) return;
            ClientSetSkin(_skinDTO);
        }

        [Client]
        public void ClientSetSkin(SkinDto skin)
        {
            if(skin.bangs == null || skin.eyeColorB1 == new Color(0,0,0,0))
            {
                Debug.Log($"Skin is null");
                return;
            }
            Debug.Log($"ClientSetSkin called on {gameObject.name} (isClient={isClient}, isOwned={isOwned}");
            var json = JsonUtility.ToJson(skin);
            CmdSetSkin(json);
        }
        
        
        [Command(requiresAuthority = false)]
        private void CmdSetSkin(string skin)
        {
            Debug.Log($"CmdSetSkin received on server (isServer={isServer})");
            _jsonSkinDTO = skin;
        }
        
        private void OnSkinDTOChanged(string oldSkin, string newSkin)
        {
            Debug.Log($"OnSkinDTOChanged called (isClient={isClient}), new skin: {newSkin}");
            var skin = JsonUtility.FromJson<SkinDto>(newSkin);
            ApplySkin(skin);
        }

        private void ApplySkin(SkinDto skinDto)
        {
            _skinDTO = skinDto;
            SetColors(_skinDTO);
            SetBodyPartActive(_lowerArms, !skinDto.disabledParts.HasFlag(BodyPart.LowerArms));
            SetBodyPartActive(_upperArms, !skinDto.disabledParts.HasFlag(BodyPart.UpperArms));
            SetBodyPartActive(_feet, !skinDto.disabledParts.HasFlag(BodyPart.Feet));
            SetBodyPartActive(_hands, !skinDto.disabledParts.HasFlag(BodyPart.Hands));
            SetBodyPartActive(_hips, !skinDto.disabledParts.HasFlag(BodyPart.Hips));
            SetBodyPartActive(_legsKnee, !skinDto.disabledParts.HasFlag(BodyPart.LegsKnee));
            SetBodyPartActive(_legsLower, !skinDto.disabledParts.HasFlag(BodyPart.LegsLower));
            SetBodyPartActive(_legsUpper, !skinDto.disabledParts.HasFlag(BodyPart.LegsUpper));
            SetBodyPartActive(_shoulders, !skinDto.disabledParts.HasFlag(BodyPart.Shoulders));
            SetBodyPartActive(_torso, !skinDto.disabledParts.HasFlag(BodyPart.Torso));
            SetBodyPartActive(_neck, !skinDto.disabledParts.HasFlag(BodyPart.Neck));
            SetBodyPartActive(_head, !skinDto.disabledParts.HasFlag(BodyPart.Head));
            SetBodyPartActive(_hairParent, !skinDto.disabledParts.HasFlag(BodyPart.Hair));
            SetBodyPartActive(_bangsParent, !skinDto.disabledParts.HasFlag(BodyPart.Bangs));
            SetTop(skinDto.top);
            SetBottom(skinDto.bottom);
            SetShoes(skinDto.shoes);
            SetHair(skinDto.hair);
            SetBangs(skinDto.bangs);
            SetGlasses(skinDto.glasses);
        }

        private void SetColors(SkinDto skinDto)
        {
            foreach (var material in _skinMaterials)
                material.SetColor("_Color_A_2", skinDto.skinColor);
            foreach (var material in _hairMaterials)
            {
                material.SetColor(ColorA1, skinDto.hairColorA1);
                material.SetColor(ColorA2, skinDto.hairColorA2);
            }
            _eyeMaterial.SetColor(ColorB1, skinDto.eyeColorB1);
            _eyeMaterial.SetColor(ColorB2, skinDto.eyeColorB2);
            _eyeMaterial.SetColor(ColorC1, skinDto.eyeColorC1);
        }

        private void SetBodyPartActive(Transform bodyPart, bool isActive)
        {
            if (bodyPart != null)
                bodyPart.gameObject.SetActive(isActive);
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
    }
}