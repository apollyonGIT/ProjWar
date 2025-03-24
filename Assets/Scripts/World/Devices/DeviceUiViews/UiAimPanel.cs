﻿using UnityEngine;

namespace World.Devices.DeviceUiViews
{
    public class UiAimPanel : MonoBehaviour
    {
        public DeviceUiView duv;
        public UiAim ui_aim;

        public void SetTarget(ITarget t)
        {
            duv.owner.target_list.Add(t);
        }
    }
}
