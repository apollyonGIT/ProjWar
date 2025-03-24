﻿using UnityEngine;

namespace World.Devices.DeviceEmergencies
{
    public class DeviceFired : DeviceEmergency
    {
        public float fire_value;
        public float fire_value_max = 100;
        public const int fire_interval = 24;
        public int fire_interval_current;

        public Device owner;

        public DeviceFired(float fire_value)
        {
            this.fire_value = fire_value;
            fire_interval_current = fire_interval;
        }

        public override void tick()
        {
            if(fire_value <= 0)
            {
                extinguish();
            }

            fire_interval_current--;
            if(fire_interval_current <= 0)
            {
                fire_interval_current = fire_interval;

                fire_value = fire_value * (0.005f * (210 + WorldContext.instance.caravan_velocity.magnitude));

                fire_value = Mathf.Min(fire_value, fire_value_max);

                owner.Hurt((int)(fire_value/5000f * owner.desc.hp));
            }
        }

        public void AddFire(int value)
        {
            fire_value += value;
            fire_value = Mathf.Min(fire_value, fire_value_max);
        }

        private void extinguish()
        {
            removed = true;
        }
    }
}
