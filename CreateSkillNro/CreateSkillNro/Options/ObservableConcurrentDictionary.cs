using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace CreateSkillNro.Options;

public class ObservableConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<KeyValuePair<TKey, TValue>> ItemAdded;
    public event EventHandler<TKey> ItemRemoved;

    public new TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
    {
        var isNew = !ContainsKey(key);
        var result = base.AddOrUpdate(key, addValue, updateValueFactory);

        if (isNew) OnItemAdded(new KeyValuePair<TKey, TValue>(key, result));

        OnPropertyChanged(nameof(Keys));
        OnPropertyChanged(nameof(Values));
        OnPropertyChanged(nameof(Count));
        return result;
    }

    public new bool TryAdd(TKey key, TValue value)
    {
        var result = base.TryAdd(key, value);
        if (result)
        {
            OnItemAdded(new KeyValuePair<TKey, TValue>(key, value));
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(Count));
        }

        return result;
    }

    public new bool TryRemove(TKey key, out TValue value)
    {
        var result = base.TryRemove(key, out value);
        if (result)
        {
            OnItemRemoved(key);
            OnPropertyChanged(nameof(Keys));
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(Count));
        }

        return result;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
    }

    protected virtual void OnItemAdded(KeyValuePair<TKey, TValue> item)
    {
        Dispatcher.UIThread.InvokeAsync(() => ItemAdded?.Invoke(this, item));
    }

    protected virtual void OnItemRemoved(TKey key)
    {
        Dispatcher.UIThread.InvokeAsync(() => ItemRemoved?.Invoke(this, key));
    }
}