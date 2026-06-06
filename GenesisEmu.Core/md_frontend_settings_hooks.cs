namespace MDTracer
{
    //----------------------------------------------------------------
    // Frontend settings persistence hooks.
    //
    // md_main_setting delegates window-layout keys to this interface
    // so the core can load and save settings without owning Form_*.
    //----------------------------------------------------------------

    internal interface IFrontendSettingsHooks
    {
        bool TryApplySetting(string in_name, string in_value);
        void CaptureSettings(Action<string, string> settingAdd);
        void EnsureCodeToolLayoutVisible();
    }

    internal sealed class NullFrontendSettingsHooks : IFrontendSettingsHooks
    {
        public bool TryApplySetting(string in_name, string in_value) => false;
        public void CaptureSettings(Action<string, string> settingAdd) { }
        public void EnsureCodeToolLayoutVisible() { }
    }
}
