using Foundations.DialogGraphs;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class JumpNode : DialogNode
    {
        public TextField target_uname_content;

        static Vector2 node_size = new(150, 200);

        public override Type node_type => typeof(JumpNode);
        public override Type coder_type => typeof(JumpNode_Coder);

        //==================================================================================================

        public static void create_node(DialogNode_Data data, DialogGraphView view)
        {
            var node = create_node(data.node_name, new(data.pos.Item1, data.pos.Item2), view);
            node._desc = data;
            node.uname_content.value = data.uname;

            node.target_uname_content.value = (string)data.fields["target_uname"];

            view.AddElement(node);
        }


        public static JumpNode create_node(string nodeName, Vector2 pos, DialogGraphView view)
        {
            JumpNode node = new()
            {
                title = nodeName,
            };
            node._desc.GUID = Guid.NewGuid().ToString();
            node._desc.node_name = nodeName;
            node._desc.node_type = node.node_type;
            node._desc.coder_type = node.coder_type;

            var inputPort = DialogGraphEditor_Utility.create_port(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            #region uname
            DialogGraphEditor_Utility.create_uname_area(node);
            #endregion

            DialogGraphEditor_Utility.create_input_prm(node, ref node.target_uname_content, "【目标节点】", "target_uname");

            node.RefreshExpandedState();
            node.RefreshPorts();

            pos = view.contentViewContainer.WorldToLocal(pos);
            node.SetPosition(new Rect(pos, node_size));

            return node;
        }
    }
}

