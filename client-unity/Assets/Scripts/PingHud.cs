using UnityEngine;
using System;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;

#nullable enable
public sealed class PingHud : MonoBehaviour
{
    public static PingHud Instance { get; private set; }

    public DbConnection? Conn;
    public float PingIntervalSeconds = 0.5f;

    private uint NextSequence;
    private readonly Dictionary<uint, double> SendTimeSecondsBySequence = new Dictionary<uint, double>();

    private double NextSendTimeSeconds;

    private float LastRttMilliseconds;
    private float SmoothedRttMilliseconds;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        NextSendTimeSeconds = Time.realtimeSinceStartupAsDouble + PingIntervalSeconds;
    }

    private void Update()
    {
        if (Conn == null) return;

        double CurrentTimeSeconds = Time.realtimeSinceStartupAsDouble;

        if (CurrentTimeSeconds >= NextSendTimeSeconds)
        {
            SendPing(CurrentTimeSeconds);
            NextSendTimeSeconds = CurrentTimeSeconds + PingIntervalSeconds;
        }
    }

    public void Initialize(DbConnection Connection)
    {
        Conn = Connection;

        Conn.Db.PingStatus.OnInsert += HandlePingStatusInsert;
        Conn.Db.PingStatus.OnUpdate += HandlePingStatusUpdate;

        foreach (PingStatus Row in Conn.Db.PingStatus.Iter())
        {
            HandlePingStatusRow(Row);
        }
    }

    private void SendPing(double CurrentTimeSeconds)
    {
        uint Sequence = NextSequence;
        NextSequence = NextSequence + 1;

        SendTimeSecondsBySequence[Sequence] = CurrentTimeSeconds;

        if (SendTimeSecondsBySequence.Count > 64)
        {
            SendTimeSecondsBySequence.Clear();
        }

        Conn!.Reducers.Ping(Sequence);
    }

    private void HandlePingStatusInsert(EventContext Context, PingStatus NewRow)
    {
        HandlePingStatusRow(NewRow);
    }

    private void HandlePingStatusUpdate(EventContext Context, PingStatus OldRow, PingStatus NewRow)
    {
        HandlePingStatusRow(NewRow);
    }

    private void HandlePingStatusRow(PingStatus Row)
    {
        if (Conn == null) return;
        if (!Row.Identity.Equals(Conn.Identity)) return;

        uint Sequence = Row.LastSequence;

        if (!SendTimeSecondsBySequence.TryGetValue(Sequence, out double SentTimeSeconds))
        {
            return;
        }

        SendTimeSecondsBySequence.Remove(Sequence);

        double NowSeconds = Time.realtimeSinceStartupAsDouble;
        float RttMilliseconds = (float)((NowSeconds - SentTimeSeconds) * 1000.0);

        LastRttMilliseconds = RttMilliseconds;

        if (SmoothedRttMilliseconds <= 0.0f)
        {
            SmoothedRttMilliseconds = RttMilliseconds;
        }
        else
        {
            SmoothedRttMilliseconds = SmoothedRttMilliseconds * 0.8f + RttMilliseconds * 0.2f;
        }
    }

    private void OnGUI()
    {
        if (Conn == null || SmoothedRttMilliseconds <= 0.0f)
        {
            GUI.Label(new Rect(10, 10, 320, 24), "Ping: -- ms");
            return;
        }

        GUI.Label(new Rect(10, 10, 320, 24), $"Ping: {SmoothedRttMilliseconds:0} ms (last {LastRttMilliseconds:0})");
    }
}
