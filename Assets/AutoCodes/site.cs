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
    
    
    public class site
    {
        
        public uint id;
        
        public Foundations.Excels.Site_Type type;
    }
    
    public class sites
    {
        
        private static System.Collections.Generic.Dictionary<string, site> m_records;
        
        public static System.Collections.Generic.Dictionary<string, site> records
        {
            get
            {
                return (System.Collections.Generic.Dictionary<string, site>)Foundations.Excels.ExcelAnalyzer.init("site");
            }
        }
        
        public static bool TryGetValue(string key, out site record)
        {
            return Foundations.Excels.ExcelAnalyzer.try_get_value("site", key, out record);
        }
    }
}
