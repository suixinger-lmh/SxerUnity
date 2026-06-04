

namespace Sxer.Plugin.SaveSystem
{
    public interface ISaveDataProvider<T> 
    {        
        T GenerateSaveData();

        void SetupSaveData(T data);

    }
}