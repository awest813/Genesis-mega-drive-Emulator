namespace MDTracer
{
    internal sealed class WinFormsIoFrontendHooks : IIoFrontendHooks
    {
        public void NotifyJoystickDevicesChanged()
        {
            WinFormsDebugTools.g_form_io?.update_joystick_combo_from_device_scan();
        }
    }
}
