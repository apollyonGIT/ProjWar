namespace World.Relic.Relics
{
    public class AddSalvoRelic : Relic
    {
        public override void Get()
        {
            BattleContext.instance.projectile_salvo_amount += 1;
        }

        public override void Drop()
        {
            BattleContext.instance.projectile_salvo_amount -= 1;
        }
    }
}
