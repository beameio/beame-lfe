using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class PluginEnums
    {

        public enum ePluginType 
        {
            [Description("Wix")] WIX             = 1,
            [Description("Facebook")] FB         = 2,
            [Description("WordPress")] WORDPRESS = 3,
            [Description("Joomla")] JOOMLA       = 4,
            [Description("Drupal")] DRUPAL       = 5
        }

    }
}
