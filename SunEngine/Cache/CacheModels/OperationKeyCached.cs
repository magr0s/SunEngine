using SunEngine.Models.Authorization;

namespace SunEngine.Stores.CacheModels
{
    public class OperationKeyCached
    {
        public int OperationKeyId { get;  }
        public string Name { get;  }
        
        public OperationKeyCached(OperationKey operationKey)
        {
            OperationKeyId = operationKey.OperationKeyId;
            Name = operationKey.Name;
        }
    }
}