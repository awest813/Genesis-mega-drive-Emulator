namespace MDTracer
{
    //----------------------------------------------------------------
    // Optional I/O frontend notification hooks.
    //
    // md_io_device previously reached into md_main.g_form_io when a
    // joystick rescan detected new hardware. This hook keeps device
    // scanning in the core while UI refresh stays in the frontend.
    //----------------------------------------------------------------

    internal interface IIoFrontendHooks
    {
        void NotifyJoystickDevicesChanged();
    }

    internal sealed class NullIoFrontendHooks : IIoFrontendHooks
    {
        public void NotifyJoystickDevicesChanged() { }
    }

    internal sealed class WinFormsIoFrontendHooks : IIoFrontendHooks
    {
        public void NotifyJoystickDevicesChanged()
        {
            WinFormsDebugTools.g_form_io?.update_joystick_combo_from_device_scan();
        }
    }
}
