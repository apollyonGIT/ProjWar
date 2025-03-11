﻿using System;
using System.Collections.Generic;

namespace Foundations.Tickers
{
    public class Request
    {
        public string req_name;

        Func<Request, bool> m_condition;
        Action<Request> m_ac_do_end;
        Action<Request> m_ac_do_tick;

        public int countdown;
        public Dictionary<string, object> prms_dic = new();

        bool m_is_break;

        //==================================================================================================

        public Request(string req_name, Func<Request, bool> condition, Action<Request> ac_do_start = null, Action<Request> ac_do_end = null, Action<Request> ac_do_tick = null)
        {
            this.req_name = req_name;

            m_condition = condition;
            m_ac_do_end = ac_do_end;
            m_ac_do_tick = ac_do_tick;

            ac_do_start?.Invoke(this);

            Ticker.instance.do_when_tick_request += tick;
        }


        public void tick()
        {
            if (m_is_break)
            {
                Ticker.instance.clean_req(this);
                return;
            }

            var is_end = m_condition.Invoke(this);
            if (is_end)
            {
                m_ac_do_end?.Invoke(this);
                Ticker.instance.clean_req(this);
                return;
            }

            m_ac_do_tick?.Invoke(this);
        }


        public void @interrupt()
        {
            m_is_break = true;
        }
    }


    public class Request_Helper
    {
        public static Request delay_do(string req_name, int cd, Action<Request> ac_do_end = null)
        {
            Request req = new(req_name,
                        (req) => { return req.countdown == 0; },
                        (req) => { req.countdown = cd; },
                        (req) =>
                        {
                            ac_do_end?.Invoke(req);
                        },
                        (req) => { req.countdown--; }
                        );

            return req;
        }


        public static IEnumerable<Request> query_request(string req_name_contain)
        {
            return Ticker.instance.query_req(req_name_contain);
        }
    }
}

