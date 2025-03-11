﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Foundations.DialogGraphs
{
    [Serializable]
    public class DialogNode_Data
    {
        public string GUID;
        public string node_name;
        public string uname;

        public Dictionary<string, object> fields = new();

        public DialogNode_Port_Data[] ports;

        public (float, float) pos;
        public Type node_type;
        public Type coder_type;

        //==================================================================================================

        public DialogNode_Data clone()
        {
            return (DialogNode_Data)MemberwiseClone();
        }
    }


    [Serializable]
    public struct DialogNode_Port_Data
    {
        public string portName;
    }


    [Serializable]
    public class Edge_Data
    {
        public string GUID;

        public string o_node_GUID;
        public int o_portIndex;
        public string o_portName;

        public string i_node_GUID;

        public bool is_entry;

        //==================================================================================================

        public Edge_Data clone()
        {
            return (Edge_Data)MemberwiseClone();
        }
    }
}

