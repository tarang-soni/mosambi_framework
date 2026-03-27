namespace Mosambi.Core.Pooling
{
    public interface IPoolable
    {
        // Called when the object is pulled from the pool
        void OnSpawn();
        
        // Called when the object is returned to the pool
        void OnDespawn();
    }
}