namespace GameRayBurst.FtLinq.Recompiler.Enumerable
{
    internal sealed class ElementAt : ElementSelectorImpl
    {
        public ElementAt()
            : base("ElementAt", true, ElementSelectorMode.ElementAt)
        { }
    }

    internal sealed class ElementAtOrDefault : ElementSelectorImpl
    {
        public ElementAtOrDefault()
            : base("ElementAtOrDefault", false, ElementSelectorMode.ElementAt)
        { }
    }
}
