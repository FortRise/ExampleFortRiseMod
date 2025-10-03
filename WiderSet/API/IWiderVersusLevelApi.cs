using FortRise;

namespace Teuria.WiderSet;

public partial interface IWiderSetModApi
{
    public interface IWiderVersusLevelApi
    {
        void RegisterWideLevelsForVersusTower(string levelID, IResourceInfo[] levels);
    }
}

