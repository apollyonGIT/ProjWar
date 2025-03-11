namespace World.Devices.DeviceUiViews
{
    public class BasicMeleeUiView:DeviceUiView
    {
        public void ControlDevice()
        {
            InputController.instance.SetDeviceControl(owner);
        }
    }
}
