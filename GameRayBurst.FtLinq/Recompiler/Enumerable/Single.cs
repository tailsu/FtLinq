namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Single : ElementSelectorImpl
    {
        public Single()
            : base("Single", true, ElementSelectorMode.Single)
        { }
    }

    internal sealed class SingleOrDefault : ElementSelectorImpl
    {
        public SingleOrDefault()
            : base("SingleOrDefault", false, ElementSelectorMode.Single)
        { }
    }
}
