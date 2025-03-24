namespace World.Devices.DeviceUiViews
{
    public class BasicHookUiView : DeviceUiView
    {
        public HookTurnTable htt;
        public override void notify_on_tick()
        {
            base.notify_on_tick();

            htt.tick();
        }
    }
}
