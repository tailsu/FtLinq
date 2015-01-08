namespace GameRayBurst.FtLinq.Recompiler
{
    public interface IMethod<T>
    {
        bool CanHandle(T input);
    }
}
