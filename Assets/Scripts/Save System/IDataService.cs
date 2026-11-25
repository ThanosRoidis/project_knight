
namespace Shared
{
    public interface IDataService
    {
        bool SaveData<T>(string path, T Data, bool encrypted);

        T LoadData<T>(string path, bool encrypted);

    }
}
