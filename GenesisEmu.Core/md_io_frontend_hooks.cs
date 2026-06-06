namespace MDTracer
{
    //----------------------------------------------------------------
    // Optional I/O frontend notification hooks.
    //
    // md_io_device notifies the frontend when a joystick rescan
    // detects new hardware. Production wires in a WinForms hook;
    // headless/tests use NullIoFrontendHooks.
    //----------------------------------------------------------------

    internal interface IIoFrontendHooks
    {
        void NotifyJoystickDevicesChanged();
    }

    internal sealed class NullIoFrontendHooks : IIoFrontendHooks
    {
        public void NotifyJoystickDevicesChanged() { }
    }
}
