using Foundations.DialogGraphs;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DialogNode : Node
    {
        public TextField uname_content;

        public DialogNode_Data _desc = new();

        public virtual Type node_type => null;
        public virtual Type coder_type => null;

        //==================================================================================================

    }
}

