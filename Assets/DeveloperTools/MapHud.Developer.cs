using Members.CJY.Scripts;

namespace Members.KJY._01.Scripts.UI
{
    public partial class MapHud
    {
        internal void DeveloperRefresh(NodeMaker map)
        {
            if (nodeMaker == map && _storage != null) Refresh(null);
        }
    }
}
