using System.Collections.Generic;

namespace AkashaScanner.Core
{
    public interface IAchievementConfig : IBaseScrapConfig
    {
        Dictionary<int, bool> AchievementOverrides { get; set; }
    }
}
