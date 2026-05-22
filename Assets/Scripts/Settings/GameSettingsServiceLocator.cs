namespace Core.Settings
{
    public static class GameSettingsServiceLocator
    {
        private static GameSettingsService _instance;

        public static GameSettingsService Instance => _instance ??= new GameSettingsService();

        public static void SetInstance(GameSettingsService service)
        {
            if (service != null)
                _instance = service;
        }
    }
}
