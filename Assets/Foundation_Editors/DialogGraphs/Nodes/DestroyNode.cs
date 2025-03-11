using Foundations.DialogGraphs;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DestroyNode : DialogNode
    {
        static Vector2 node_size = new(150, 200);

        public override Type node_type => typeof(DestroyNode);
        public override Type coder_type => typeof(DestroyNode_Coder);

        //==================================================================================================

        public static void create_node(DialogNode_Data data, DialogGraphView view)
        {
            var node = create_node(data.node_name, new(data.pos.Item1, data.pos.Item2), view);
            node._desc = data;
            node.uname_content.value = data.uname;

            view.AddElement(node);
        }


        public static DestroyNode create_node(string nodeName, Vector2 pos, DialogGraphView view)
        {
            DestroyNode node = new()
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
            TextField uname_content = new();
            uname_content.RegisterValueChangedCallback((evt) =>
            {
                node._desc.uname = evt.newValue;
            });
            node.titleContainer.Add(uname_content);
            node.uname_content = uname_content;
            #endregion

            node.RefreshExpandedState();
            node.RefreshPorts();

            pos = view.contentViewContainer.WorldToLocal(pos);
            node.SetPosition(new Rect(pos, node_size));

            return node;
        }
    }
}

