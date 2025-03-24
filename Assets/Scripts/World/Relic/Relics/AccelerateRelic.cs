﻿namespace World.Relic.Relics
{
    public class AccelerateRelic : Relic
    {
        public override void Get()
        {
            WorldContext.instance.caravan_velocity += new UnityEngine.Vector2(10, 0);
        }

        public override void Drop()
        {
            WorldContext.instance.caravan_velocity -= new UnityEngine.Vector2(10, 0);
        }
    }
}
