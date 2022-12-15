namespace AkashaScanner.Ui.StateManagerment
{
    public partial class Flux
    {
        private readonly Dictionary<Type, DataCollectionStatus> DataCollectionStatuses = new();

        [Dispatcher]
        private void OnSetDataCollectionStatus(SetDataCollectionStatusAction action)
        {
            DataCollectionStatuses[action.EntryType] = new DataCollectionStatus()
            {
                IsLoading = action.IsLoading,
                IsOutdated = action.IsOutdated,
                LastUpdate = action.LastUpdate,
            };
            Notify<GetDataCollectionStatus>(proj => proj.EntryType == action.EntryType);
        }

        public record DataCollectionStatus
        {
            public bool IsLoading;
            public bool IsOutdated;
            public DateTime LastUpdate;
            public bool IsAvailable => !IsLoading && !IsOutdated;
        }

        public class GetDataCollectionStatus : Projection
        {
            public readonly Type EntryType;
            public GetDataCollectionStatus(Type entryType)
            {
                EntryType = entryType;
            }

            public override object? GetValue(Flux flux)
            {
                DataCollectionStatus? status;
                if (!flux.DataCollectionStatuses.TryGetValue(EntryType, out status))
                {
                    status = new DataCollectionStatus() { IsLoading = true };
                }
                return status;
            }
        }
    }
}
