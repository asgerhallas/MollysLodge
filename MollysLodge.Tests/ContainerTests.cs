using MollysLodge;
using Shouldly;

namespace MollyLodge.Tests;

public class ContainerTests
{
    [Fact]
    public void Resolve()
    {
        var container = new Container();

        container.Register<Item1>(_ => new Item1());
        container.Register<Item2>(_ => new Item2());

        container.Resolve<Item1>().ShouldNotBe(null);
        container.Resolve<Item1>().ShouldNotBe(null);
    }

    [Fact]
    public void Resolve_NotRegistered()
    {
        var container = new Container();

        Should.Throw<InvalidOperationException>(() => container.Resolve<object>())
            .Message.ShouldBe("No activator registered for 'System.Object'.");
    }

    [Fact]
    public void Resolve_InvokeFactoryOnlyOnce()
    {
        var container = new Container();

        var invokeCounter = 1;

        container.Register<Item1>(_ =>
        {
            invokeCounter.ShouldBe(1);

            invokeCounter++;

            return new Item1();
        });

        container.Resolve<Item1>().ShouldNotBe(null);
        container.Resolve<Item1>().ShouldNotBe(null);
    }

    [Fact]
    public void Register_AlreadyResolved()
    {
        var container = new Container();

        container.Register<Item1>(_ => new Item1());

        _ = container.Resolve<Item1>();

        Should.Throw<InvalidOperationException>(() => container.Register<Item1>(_ => new Item1()))
            .Message.ShouldBe("Type 'MollyLodge.Tests.ContainerTests+Item1' has already been resolved.");
    }

    [Fact]
    public void Register_NoFactory()
    {
        var container = new Container();

        Should.Throw<ArgumentNullException>(() => container.Register<object>(null))
            .Message.ShouldBe("Value cannot be null. (Parameter 'factory')");
    }

    [Fact]
    public void Register_WithOverwrite()
    {
        var container = new Container();

        container.Register<Item1>(_ => new Item1(), overwriteExisting: true).ShouldBe(true);

        // The second registration overwrites the first one.
        container.Register<Item1>(_ => new Item2(), overwriteExisting: true).ShouldBe(true);
    }

    [Fact]
    public void Register_WithoutOverwrite()
    {
        var container = new Container();

        container.Register<Item1>(_ => new Item1(), overwriteExisting: false).ShouldBe(true);

        // The second registration does not overwrite the first one.
        container.Register<Item1>(_ => new Item2(), overwriteExisting: false).ShouldBe(false);
    }

    [Fact]
    public void Decorate_AlreadyResolved()
    {
        var container = new Container();

        container.Register<Item1>(_ => new Item1());

        _ = container.Resolve<Item1>();

        Should.Throw<InvalidOperationException>(() => container.Decorate<Item1>((_, _) => new Item1()))
            .Message.ShouldBe("Type 'MollyLodge.Tests.ContainerTests+Item1' has already been resolved.");
    }

    [Fact]
    public void Decorate_NoFactory()
    {
        var container = new Container();

        Should.Throw<ArgumentNullException>(() => container.Decorate<object>(null))
            .Message.ShouldBe("Value cannot be null. (Parameter 'factory')");
    }

    [Fact]
    public void Decorate_NotRegistered()
    {
        var container = new Container();

        Should.Throw<InvalidOperationException>(() => container.Decorate<object>((_, _) => null!))
            .Message.ShouldBe("There's no 'System.Object' to decorate in the container.");
    }

    [Fact]
    public void Decorate()
    {
        var container = new Container();

        var item = new Item1();

        container.Register<Item1>(_ => item);

        container.Decorate<Item1>((_, x) =>
        {
            x.ShouldBe(item);

            return x;
        });
    }

    [Fact]
    public void TryResolve()
    {
        var container = new Container();

        container.Register<Item1>(_ => new Item1());
        container.Register<Item2>(_ => new Item2());

        container.TryResolve<Item1>(out var item1).ShouldBe(true);
        container.TryResolve<Item2>(out var item2).ShouldBe(true);

        item1.ShouldNotBe(null);
        item2.ShouldNotBe(null);
    }

    [Fact]
    public void TryResolve_NotRegistered()
    {
        var container = new Container();

        container.TryResolve<object>(out var item).ShouldBe(false);

        item.ShouldBe(null);
    }

    [Fact]
    public void TryResolve_InvokeFactoryOnlyOnce()
    {
        var container = new Container();

        var invokeCounter = 1;

        container.Register<Item1>(_ =>
        {
            invokeCounter.ShouldBe(1);

            invokeCounter++;

            return new Item1();
        });

        container.TryResolve<Item1>(out var item1).ShouldBe(true);
        container.TryResolve<Item1>(out var item2).ShouldBe(true);

        item1.ShouldNotBe(null);
        item2.ShouldNotBe(null);
    }

    public class Item1;

    public class Item2 : Item1;
}