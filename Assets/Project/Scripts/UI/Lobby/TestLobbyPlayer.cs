using System;
using Mirror;
using Project.Scripts.Customization;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.TEST
{
    public class TestLobbyPlayer : NetworkBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private RawImage _avatarImage;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _readyText;
        [SerializeField] private TMP_Text _leaderText;
        [SerializeField] private RawImage _showcaseImage;
        [SerializeField] private Button _readyButton;

        [SyncVar(hook = nameof(OnInputChanged))]
        private string _inputFieldValue;

        [field: SyncVar(hook = nameof(OnNameChanged))]
        public string PlayerName { get; private set; }

        [field: SyncVar(hook = nameof(OnReadyChanged))]
        public bool IsReady { get; private set; }

        [field: SyncVar(hook = nameof(OnLeaderChanged))]
        public bool IsLeader { get; private set; }

        private void Awake()
        {
            _inputField.gameObject.SetActive(false);
            _readyButton.gameObject.SetActive(false);
            _inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            _readyButton.onClick.AddListener(OnReadyButtonClicked);
        }

        public event Action<TestLobbyPlayer, bool> OnReadyChangedEvent;

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            _inputField.gameObject.SetActive(true);
            _readyButton.gameObject.SetActive(true);
        }

        //This method will be called on all clients and update the UI.
        //Because as I understand server synchronizes the values on the clients,
        //but because new values are the same as old ones they don't update UI.
        public override void OnStartClient()
        {
            base.OnStartClient();
            OnNameChanged(string.Empty, PlayerName);
            OnReadyChanged(false, IsReady);
            OnLeaderChanged(false, IsLeader);
        }

        [Client]
        private void OnInputFieldValueChanged(string value)
        {
            if (!isOwned) return;
            CmdSetInputFieldValue(value);
        }

        [Client]
        private void OnReadyButtonClicked()
        {
            if (!isOwned) return;
            CmdSetReady(!IsReady);
        }

        [Command]
        private void CmdSetInputFieldValue(string value)
        {
            _inputFieldValue = value;
        }

        [Command]
        private void CmdSetReady(bool isReady)
        {
            IsReady = isReady;
            OnReadyChangedEvent?.Invoke(this, isReady);
        }

        [Client]
        private void OnNameChanged(string oldValue, string newValue)
        {
            _nameText.text = newValue;
        }

        [Client]
        private void OnInputChanged(string oldValue, string newValue)
        {
            _resultText.text = newValue;
        }

        [Client]
        private void OnReadyChanged(bool oldValue, bool newValue)
        {
            _readyText.text = newValue ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }

        [Client]
        private void OnLeaderChanged(bool oldValue, bool newValue)
        {
            _leaderText.text = newValue ? "<color=blue>Leader</color>" : "<color=yellow>Not Leader</color>";
        }

        [Server]
        public void ServerSetPlayer(CSteamID steamID, bool isReady, bool isLeader)
        {
            PlayerName = GetName(steamID);
            IsReady = isReady;
            IsLeader = isLeader;
        }

        //This method is called in case new values of _isReady & _isLeader are the same as the old ones.
        //In this case they will not be updated and the hook will not be called.
        [Client]
        public void ClientSetPlayer(bool isReady, bool isLeader, NetworkBehaviour showcaseNetwork, CSteamID steamID)
        {
            OnNameChanged(string.Empty, GetName(steamID));
            OnReadyChanged(false, isReady);
            OnLeaderChanged(false, isLeader);
            var showcase = showcaseNetwork.GetComponent<Showcase>();
            _showcaseImage.texture = showcase.RenderTexture;
            _avatarImage.texture = GetAvatarImage(steamID);
        }

        private string GetName(CSteamID steamId)
        {
            return SteamFriends.GetFriendPersonaName(steamId);
        }

        private Texture2D GetAvatarImage(CSteamID steamId)
        {
            var imageId = SteamFriends.GetLargeFriendAvatar(steamId);
            if (imageId == -1) return null;

            if (!SteamUtils.GetImageSize(imageId, out var width, out var height)) return null;

            var imageData = new byte[width * height * 4];
            if (!SteamUtils.GetImageRGBA(imageId, imageData, (int)(width * height * 4))) return null;

            var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, false); // sRGB enabled
            texture.LoadRawTextureData(imageData);
            texture.Apply();

            // Повертаємо Y-вісь
            var pixels = texture.GetPixels32();
            var flippedPixels = new Color32[pixels.Length];
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                flippedPixels[(int)((height - 1 - y) * width + x)] = pixels[(int)(y * width + x)];

            texture.SetPixels32(flippedPixels);
            texture.Apply();

            return texture;
        }
    }
}