namespace World.Devices.DeviceUiViews
{
    public class BasicShieldUIView : DeviceUiView
    {
        public void ControlDevice()
        {
            InputController.instance.SetDeviceControl(owner);
        }
    }
}
