using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    private static Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

    // Subscribe
    public static void Subscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (_events.ContainsKey(type))
            _events[type] = Delegate.Combine(_events[type], callback);
        else
            _events[type] = callback;
    }

    // Unsubscribe
    public static void Unsubscribe<T>(Action<T> callback)
    {
        var type = typeof(T);
        if (_events.ContainsKey(type))
        {
            _events[type] = Delegate.Remove(_events[type], callback);
            if (_events[type] == null) _events.Remove(type);
        }
    }

    // Publish
    public static void Publish<T>(T message)
    {
        var type = typeof(T);
        if (_events.ContainsKey(type))
        {
            var callback = _events[type] as Action<T>;
            callback?.Invoke(message);
        }
    }
}