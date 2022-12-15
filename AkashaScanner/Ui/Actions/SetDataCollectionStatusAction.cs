namespace AkashaScanner.Ui.StateManagerment
{
    public record SetDataCollectionStatusAction : IAction
    {
        public Type EntryType = default!;
        public bool IsOutdated;
        public bool IsLoading;
        public DateTime LastUpdate;
    }
}
