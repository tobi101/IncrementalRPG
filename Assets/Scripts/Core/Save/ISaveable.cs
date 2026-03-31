namespace Core.Save
{
    public interface ISaveable
    {
        void Contribute(SaveData data);
        void Load(SaveData data);
    }
}
