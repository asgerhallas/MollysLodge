namespace MollysLodge;

public interface IContainerActivator
{
    T Resolve<T>();
}