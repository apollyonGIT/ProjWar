using System;
using System.Collections.Generic;
using UnityEngine;

namespace Foundations.DialogGraphs
{
    public class DestroyNode_Coder : IDialogNode_Coder
    {
        Dictionary<int, Func<object>> m_outputs = new();
        Func<object[], object> m_input;
        Dictionary<string, object> m_fields = new();

        Dictionary<int, Func<object>> IDialogNode_Coder.outputs { get => m_outputs; set => m_outputs = value; }
        Func<object[], object> IDialogNode_Coder.input { get => m_input; set => m_input = value; }
        Dictionary<string, object> IDialogNode_Coder.fields { get => m_fields; set => m_fields = value; }

        string IDialogNode_Coder.uname => m_uname;
        string m_uname = "";

        string IDialogNode_Coder.key_name => "DestroyNode";

        //==================================================================================================

        void IDialogNode_Coder.notify_on_init(params object[] args)
        {
            var data = (DialogNode_Data)args[0];
            m_uname = data.uname;
        }


        object IDialogNode_Coder.do_output(int index, params object[] args)
        {
            return null;
        }


        object IDialogNode_Coder.do_input(params object[] args)
        {
            return m_input?.Invoke(null);
        }
    }
}

