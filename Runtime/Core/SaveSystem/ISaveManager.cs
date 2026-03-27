namespace Mosambi.Core
{
    public interface ISaveManager<T> where T : new()
    {
        T Data { get; }
        void Save();
        void Load();
        void ClearData();
    }
}