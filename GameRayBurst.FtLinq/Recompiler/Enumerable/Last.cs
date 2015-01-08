namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class Last : ElementSelectorImpl
    {
        public Last()
            : base("Last", true, ElementSelectorMode.Last)
        { }
    }

    internal sealed class LastOrDefault : ElementSelectorImpl
    {
        public LastOrDefault()
            : base("LastOrDefault", false, ElementSelectorMode.Last)
        { }
    }
}