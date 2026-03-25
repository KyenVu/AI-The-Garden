using System;
using System.Collections.Generic;
using UnityEngine;

public class AILogger : MonoBehaviour
{
    public static AILogger Instance { get; private set; }

    [Serializable]
    public class LogEntry
    {
        public float time;
        public string channel;
        public string message;
    }

    // All entries (you can cap this if you want)
    public List<LogEntry> entries = new List<LogEntry>();

    // Channel  enabled/disabled
    private readonly Dictionary<string, bool> _channelEnabled = new Dictionary<string, bool>();

    // Events for the UI
    public event Action<LogEntry> OnLogAdded;
    public event Action OnChannelsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("AILogger: Another instance already exists. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Log a message to a named channel.
    /// Example: AILogger.Log("EnemyFSM", "State changed to Chase");
    /// </summary>
    public static void Log(string channel, string message)
    {
        if (Instance == null)
        {
            Debug.LogWarning($"AILogger: No instance in the scene. Log skipped. [{channel}] {message}");
            return;
        }

        Instance.InternalLog(channel, message);
    }

    private void InternalLog(string channel, string message)
    {
        // Register new channels automatically
        if (!_channelEnabled.ContainsKey(channel))
        {
            _channelEnabled[channel] = true;  // default: ON
            OnChannelsChanged?.Invoke();
        }

        var entry = new LogEntry
        {
            time = Time.time,
            channel = channel,
            message = message
        };

        entries.Add(entry);

        // Forward to UI only if the channel is currently enabled
        if (IsChannelEnabled(channel))
        {
            OnLogAdded?.Invoke(entry);
        }

        // Optional: also push to Unity console for quick debugging
        Debug.Log($"[AI][{channel}] {message}");
    }

    public bool IsChannelEnabled(string channel)
    {
        return _channelEnabled.TryGetValue(channel, out bool enabled) && enabled;
    }

    public void SetChannelEnabled(string channel, bool enabled)
    {
        if (!_channelEnabled.ContainsKey(channel))
            _channelEnabled[channel] = enabled;
        else
            _channelEnabled[channel] = enabled;

        // When filters change, UI may want to rebuild the visible log text
        OnChannelsChanged?.Invoke();
    }

    public IEnumerable<string> GetChannels()
    {
        return _channelEnabled.Keys;
    }
}

