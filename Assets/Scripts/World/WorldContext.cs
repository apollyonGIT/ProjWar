using Foundations;
using Foundations.Tickers;

namespace World
{
    public class WorldContext : Singleton<WorldContext>
    {

        //==================================================================================================

        public void attach()
        {
            Ticker.instance.do_when_tick_start += ctx_tick_start;
            Ticker.instance.do_when_tick_end += ctx_tick_end;
        }
        

        public void detach()
        {

        }


        private void ctx_tick_start()
        {
            
        }


        private void ctx_tick_end()
        {
            
        }
    }
}

