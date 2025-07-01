using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MollysLodge;

public class Container : IContainerActivator, IDisposable
{
    readonly List<object> tracked = new();
    readonly ConcurrentDictionary<Type, Lazy<object>> factories = new();

    public T Resolve<T>()
    {
        if (!TryResolve<T>(out var result))
        {
            throw new InvalidOperationException($"No activator registered for '{typeof(T)}'.");
        }

        return result;
    }

    public void Dispose()
    {
        foreach (var disposable in tracked.OfType<IDisposable>())
        {
            disposable.Dispose();
        }
    }

    public bool Register<T>(Func<IContainerActivator, T> factory, bool overwriteExisting = true)
    {
        if (factories.TryGetValue(typeof(T), out var existingActivator) && existingActivator.IsValueCreated)
        {
            throw new InvalidOperationException($"Type '{typeof(T)}' has already been resolved.");
        }

        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        var activator = new Lazy<object>(() => Track(factory(this)), isThreadSafe: true);

        var addOrUpdate = factories.AddOrUpdate(typeof(T),
            addValueFactory: _ => activator,
            updateValueFactory: (_, existing) => overwriteExisting ? activator : existing);

        return addOrUpdate == activator;
    }

    public void Decorate<T>(Func<IContainerActivator, T, T> factory)
    {
        if (factories.TryGetValue(typeof(T), out var existingActivator) && existingActivator.IsValueCreated)
        {
            throw new InvalidOperationException($"Type '{typeof(T)}' has already been resolved.");
        }

        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        factories.AddOrUpdate(typeof(T),
            addValueFactory:
            key => throw new InvalidOperationException($"There's no '{key}' to decorate in the container."),
            updateValueFactory:
            (_, activator) => new Lazy<object>(() => Track(factory(this, (T)activator.Value)!), isThreadSafe: true));
    }

    public bool TryResolve<T>(out T result)
    {
        if (!factories.TryGetValue(typeof(T), out var activator))
        {
            result = default;

            return false;
        }

        result = (T)activator.Value;

        return true;
    }

    T Track<T>(T obj)
    {
        tracked.Add(obj);

        return obj;
    }
}