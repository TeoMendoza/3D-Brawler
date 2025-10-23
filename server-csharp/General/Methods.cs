using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    static void AddSubscriberUnique(List<string> subscribers, string reason)
    {
        if (subscribers.Contains(reason)) return;
        subscribers.Add(reason);
    }

    static void RemoveSubscriber(List<string> subscribers, string reason)
    {
        for (int i = subscribers.Count - 1; i >= 0; i--)
            if (subscribers[i] == reason) { subscribers.RemoveAt(i); break; }
    }

    private static PermissionEntry GetPermissionEntry(List<PermissionEntry> entries, string key)
    {
        foreach (var entry in entries)
        {
            if (entry.Key == key)
                return entry;
        }
        return entries[0];
    }
}