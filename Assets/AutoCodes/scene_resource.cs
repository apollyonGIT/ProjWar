//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutoCodes
{
    
    
    public class scene_resource
    {
        
        public uint group_id;
        
        public uint sub_id;
        
        public float z_distance;
        
        public System.Collections.Generic.List<string> resource_path;
        
        public Foundations.Excels.Spawn_Type rnd_type;
        
        public float fixed_length;
        
        public System.ValueTuple<float, float> rnd_pos_x;
        
        public System.ValueTuple<float, float> rnd_pos_y;
        
        public float rnd_p_active;
        
        public float rnd_p_muted;
        
        public float resource_length;
        
        public System.Collections.Generic.List<bool> sub_scene;
        
        public object diy_obj;
    }
    
    public class scene_resources
    {
        
        private static System.Collections.Generic.Dictionary<string, scene_resource> m_records;
        
        public static System.Collections.Generic.Dictionary<string, scene_resource> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, scene_resource>)Foundations.Excels.ExcelAnalyzer.init("scene_resource");
            }
        }
        
        public static bool TryGetValue(string key, out scene_resource record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("scene_resource", key, out record);
        }
    }
}
