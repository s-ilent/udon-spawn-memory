using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SpawnMemory : UdonSharpBehaviour
{
    [SerializeField] private float _saveInterval = 15f;
    [SerializeField] private int _saveTimeLimitMinutes = 20;
    [Space(20)]
    [SerializeField] [UdonSynced] private Vector3 _syncedPosition;
    [SerializeField] [UdonSynced] private Quaternion _syncedRotation;
    [SerializeField] [UdonSynced] private long _lastSaveTimeTicks;

    private VRCPlayerApi _localPlayer;
    private bool _dataRestored = false;
    private bool _ownedByLocalPlayer = false;
    private bool _initialized = false;

    private static void DebugLog(string msg)
    {
        #if false
        Debug.Log($"[SpawnMemory] {msg}");
        #endif
    }

    public void Start()
    {
        DebugLog("Start.");
        _initialized = true;
        _localPlayer = Networking.LocalPlayer;
        _ownedByLocalPlayer = Networking.IsOwner(_localPlayer, gameObject);
    }

    public void _UpdatePosition()
    {
        DebugLog("Updating position...");

        if (!Utilities.IsValid(_localPlayer) || _dataRestored == false)
        {
            DebugLog("Invalid UpdatePosition!");
        }

        long currentTimeTicks = System.DateTime.UtcNow.Ticks;
        Vector3 currentPosition = _localPlayer.GetPosition();
        Quaternion currentRotation =  _localPlayer.GetRotation();

        if (Mathf.Abs(_localPlayer.GetVelocity().y) > 0.5f)
        {
            // Do nothing.
        }
        else if (Vector3.Distance(_syncedPosition, currentPosition) > 0.01f) 
        {
            _syncedPosition = currentPosition;
            _syncedRotation = currentRotation;
            _lastSaveTimeTicks = currentTimeTicks;
            RequestSerialization();
            DebugLog($"Position should be updated to {_localPlayer.GetPosition()}, {_localPlayer.GetRotation()}");
        }

        SendCustomEventDelayedSeconds("_UpdatePosition", _saveInterval);
    }


    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        DebugLog($"{gameObject.name} is restoring data for {player.displayName}...");
        if (!Utilities.IsValid(_localPlayer))
        {
            _localPlayer = Networking.LocalPlayer;
        }

        if (!_initialized) DebugLog($"{gameObject.name} WARNING! Not initialized!");
        
        // Skip this update for this GameObject if it doesn't belong to the player. 
        if (player != _localPlayer) return; 
        if (!_ownedByLocalPlayer) return; 

        DebugLog($"{gameObject.name} :: isLocalPlayer {player == _localPlayer} initialized {_initialized} owned by local player {_ownedByLocalPlayer} {Networking.GetOwner(this.gameObject).displayName}");

        _dataRestored = true;

        long currentTimeTicks = System.DateTime.UtcNow.Ticks;
        if ((currentTimeTicks - _lastSaveTimeTicks) / System.TimeSpan.TicksPerMinute <= _saveTimeLimitMinutes 
            && _syncedPosition != Vector3.zero)
        {
           _localPlayer.TeleportTo(_syncedPosition, _syncedRotation);
           DebugLog($"Restoring {player.displayName}::{_localPlayer.displayName} to {_syncedPosition}, {_syncedRotation}, isLocal: {player.isLocal}");
        }
        SendCustomEventDelayedSeconds("_UpdatePosition", _saveInterval); 
    }


}