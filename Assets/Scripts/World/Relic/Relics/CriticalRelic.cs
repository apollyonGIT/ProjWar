namespace World.Relic.Relics
{
    public class CriticalRelic : Relic
    {
        public override void Get()
        {
            BattleContext.instance.critical_chance_delta += 0.2f;
        }

        public override void Drop()
        {
            BattleContext.instance.critical_chance_delta -= 0.2f;
        }
    }
}
