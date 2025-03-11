namespace World.Relic.Relics
{
    public class ZoomProjectileRelic : Relic
    {
        public override void Get()
        {
            BattleContext.instance.projectile_scale_factor += 1f;
        }

        public override void Drop()
        {
            BattleContext.instance.projectile_scale_factor -= 1f;
        }
    }
}
