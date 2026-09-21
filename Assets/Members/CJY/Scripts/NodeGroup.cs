using _LumenLib.PoolingSystem.Runtime;
using UnityEngine;

namespace Members.CJY.Scripts
{
    public class NodeGroup : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolItemSO poolItem;
        public PoolItemSO Item => poolItem;
        public GameObject GameObject => gameObject;
        
        public void ResetItem()
        {
            
        }
    }
}