using System;
using Mirror;
using Project.Scripts.Customization;
using Project.Scripts.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Project.Scripts.UI.MainMenu
{
    public class UILobbyPlayer : NetworkBehaviour
    {
        [SerializeField] private RawImage _playerImage;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _playerStatusText;
        [SerializeField] private TMP_Text _leaderStatusText;
        [SerializeField] private RawImage _characterDisplayImage;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _startGameButton;

        // [SyncVar(hook = nameof(OnPlayerImageChanged))]
        // private Sprite _playerImageTexture;

        [SyncVar(hook = nameof(OnPlayerColorChanged))]
        private Color _playerColor;

        private SkinData _skinData;

        [field: SyncVar(hook = nameof(OnPlayerNameChanged))]
        public string PlayerName { get; private set; }

        [field: SyncVar(hook = nameof(OnReadyStatusChanged))]
        public bool IsReady { get; private set; }

        [field: SyncVar(hook = nameof(OnLeaderStatusChanged))]
        public bool IsLeader { get; private set; }

        private void Awake()
        {
            _readyButton.onClick.AddListener(OnReadyButtonClicked);
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }

        private void Start()
        {
            if (!isOwned)
            {
                _readyButton.gameObject.SetActive(false);
                _startGameButton.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _readyButton.onClick.RemoveListener(OnReadyButtonClicked);
            _startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
        }

        public event Action<UILobbyPlayer> OnStartGame;

        [Inject]
        private void Construct(SkinData skinData)
        {
            _skinData = skinData;
        }

        private void OnReadyButtonClicked()
        {
            CmdSetReadyStatus(!IsReady);
        }

        [Command]
        private void CmdSetReadyStatus(bool isReady)
        {
            IsReady = isReady;
        }

        private void OnStartGameButtonClicked()
        {
            foreach (var player in CustomNetworkManager.RoomPlayers)
                if (!player.IsReady)
                    return;
            OnStartGame?.Invoke(this);
        }

        private void OnPlayerNameChanged(string oldValue, string newValue)
        {
            _playerNameText.text = PlayerName;
        }

        private void OnPlayerColorChanged(Color oldValue, Color newValue)
        {
            _playerNameText.color = _playerColor;
        }

        /*private void OnPlayerImageChanged(Texture oldValue, Texture newValue)
        {
            _playerImage.texture = _playerImageTexture;
        }*/

        private void OnReadyStatusChanged(bool oldValue, bool newValue)
        {
            var readyText = "<color=green>Ready</color>";
            var notReadyText = "<color=red>Not Ready</color>";
            _playerStatusText.text = IsReady ? readyText : notReadyText; //
        }

        private void OnLeaderStatusChanged(bool oldValue, bool newValue)
        {
            var leaderText = "<color=blue>Host</color>";
            var notLeaderText = "<color=white>Player</color>";
            _leaderStatusText.text = IsLeader ? leaderText : notLeaderText;
            if (!isOwned) return;
            _playerStatusText.gameObject.SetActive(!IsLeader);
            _startGameButton.gameObject.SetActive(IsLeader);
            _readyButton.gameObject.SetActive(!IsLeader);
        }

        /*
        Проблема з _isLeader на клієнті:
        У коді UILobbyPlayer є [SyncVar] для _isLeader, але зміна відбувається лише на сервері через метод SetLeader().
        Проблема: SyncVar не синхронізується автоматично, якщо зміна відбувається на сервері після створення об'єкта (наприклад, у OnServerAddPlayer).
        Рішення: Додайте NetworkServer.Spawn() після зміни _isLeader або використовуйте TargetRpc для примусового оновлення клієнта.
         */
        [Server]
        public void SetLeader(bool isLeader)
        {
            IsLeader = isLeader;
        }

        [ClientRpc]
        public void RpcUpdateLeaderStatus(bool isLeader)
        {
            IsLeader = isLeader;
            OnLeaderStatusChanged(IsLeader, isLeader); // Викликаємо хук вручну
        }

        [Server]
        public void SetPlayerName(string playerName)
        {
            PlayerName = playerName;
            _playerColor = new Color(Random.value, Random.value, Random.value);
        }

        [Server]
        public void SetReadyStatus(bool isReady)
        {
            IsReady = isReady;
        }

        [ClientRpc]
        public void RpcUpdateReadyStatus(bool isReady)
        {
            IsReady = isReady;
            OnReadyStatusChanged(IsReady, isReady); // Викликаємо хук вруч}
        }

        [ClientRpc]
        public void RpcSetShowcase(NetworkBehaviour showcase)
        {
            var showcaseComponent = showcase.GetComponent<Showcase>();
            _characterDisplayImage.texture = showcaseComponent.RenderTexture;
            if (isOwned)
                showcaseComponent.Player.ClientSetSkin(ConvertToDto(_skinData));
        }

        public static SkinDto ConvertToDto(SkinData data)
        {
            var dto = new SkinDto //
            {
                skinColor = data.SkinColor.SkinItemColors[1].Color,
                hairColorA1 = data.HairColor.SkinItemColors[0].Color,
                hairColorA2 = data.HairColor.SkinItemColors[1].Color,
                eyeColorB1 = data.EyeColor.SkinItemColors[0].Color,
                eyeColorB2 = data.EyeColor.SkinItemColors[1].Color,
                eyeColorC1 = data.EyeColor.SkinItemColors[2].Color,

                top = data.GetSkinItem(SkinItemType.Top)?.ItemName ?? "",
                bottom = data.GetSkinItem(SkinItemType.Bottom)?.ItemName ?? "",
                shoes = data.GetSkinItem(SkinItemType.Shoes)?.ItemName ?? "",
                hair = data.GetSkinItem(SkinItemType.Hair)?.ItemName ?? "",
                bangs = data.GetSkinItem(SkinItemType.Bangs)?.ItemName ?? "",
                glasses = data.GetSkinItem(SkinItemType.Glasses)?.ItemName ?? "",

                disabledParts = BodyPart.None
            };
            foreach (SkinItemType type in Enum.GetValues(typeof(SkinItemType)))
            {
                var item = data.GetSkinItem(type);
                if (item != null)
                    dto.disabledParts |= item.BodyPart;
            }

            return dto;
        }
    }
}