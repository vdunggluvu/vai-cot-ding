namespace MagicMouseClone.Core
{
    public class AppConfig
    {
        public bool EnableGestures { get; set; } = true;
        public float ScrollSpeed { get; set; } = 1.0f;
        public bool ReverseScroll { get; set; } = false;
        public string ActiveProfile { get; set; } = "Default";
    }

    public interface IConfigManager
    {
        AppConfig CurrentConfig { get; }
        void Load();
        void Save();
    }

    public class InMemoryConfigManager : IConfigManager
    {
        public AppConfig CurrentConfig { get; private set; } = new AppConfig();

        public void Load()
        {
            // Stub: Load from Registry or JSON
        }

        public void Save()
        {
            // Stub: Save to Registry or JSON
        }
    }
}
