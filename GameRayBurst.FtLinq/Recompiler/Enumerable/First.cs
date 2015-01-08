namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class First : ElementSelectorImpl
    {
        public First()
            : base("First", true, ElementSelectorMode.First)
        { }
    }

    internal sealed class FirstOrDefault : ElementSelectorImpl
    {
        public FirstOrDefault()
            : base("FirstOrDefault", false, ElementSelectorMode.First)
        { }
    }
}
