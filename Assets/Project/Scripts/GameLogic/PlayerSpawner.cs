using System.Collections.Generic;
using Mirror;
using Project.Scripts.Character;
using Project.Scripts.Scene;
using Project.Scripts.TEST;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Project.Scripts.GameLogic
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private MonoBehaviour[] _interactableObjects;
        [SerializeField] private UIDoorLock _doorLock;
        private readonly List<IInteractable> _interactables = new();

        private DiContainer _diContainer;

        public List<Player> Players { get; } = new();

        private void Awake()
        {
            TestNetworkManager.OnGameStarted += SpawnPlayers;
        }

        private void OnDestroy()
        {
            TestNetworkManager.OnGameStarted -= SpawnPlayers;
        }

        [Inject]
        private void Construct(DiContainer container)
        {
            _diContainer = container;
        }

        [Server]
        private void SpawnPlayers()
        {
            Debug.Log("Game Started");
            var players = TestNetworkManager.RoomConnections;
            var spawnPointIndex = 0;
            foreach (var player in players)
            {
                var playerInstance = Instantiate(_playerPrefab);
                NetworkServer.AddPlayerForConnection(player, playerInstance.gameObject);
                var position = _spawnPoints[spawnPointIndex].position;
                spawnPointIndex++;
                _diContainer.Inject(playerInstance);
                RpcSetPlayer(playerInstance, position);
                TargetRpcSetCameraTarget(player, playerInstance);
                Players.Add(playerInstance);
            }
        }

        [ClientRpc]
        private void RpcSetPlayer(NetworkBehaviour network, Vector3 postion)
        {
            var player = network.GetComponent<Player>();
            player.transform.position = postion;
            _diContainer.Inject(player);
        }

        [TargetRpc]
        private void TargetRpcSetCameraTarget(NetworkConnection connection, NetworkBehaviour network)
        {
            foreach (var interactable in _interactableObjects)
                if (interactable is IInteractable interactableObject)
                    _interactables.Add(interactableObject);
            var player = network.GetComponent<Player>();
            var cameraTarget = new CameraTarget();
            cameraTarget.TrackingTarget = player.CameraTarget;
            _cinemachineCamera.Target = cameraTarget;
            Debug.Log($"Interactables count: {_interactables.Count}");
            player.SetUIDoorLock(_doorLock);
            foreach (var interactable in _interactables)
                interactable.InitializeLocalPlayer(player);
        }
    }
}