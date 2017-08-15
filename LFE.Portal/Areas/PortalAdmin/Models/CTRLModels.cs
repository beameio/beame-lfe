using LFE.Core.Utils;
using System.Collections.Generic;
using System.Text;

namespace LFE.Portal.Areas.PortalAdmin.Models
{
    public class CTRL_Base
    {
        private static System.Random rnd = new System.Random();
        public CTRL_Base()
        {
            const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var sb = new StringBuilder();
            for (var i = 0; i < 10; i++)
                sb.Append(str[rnd.Next(0, str.Length)]);
            _id = string.Format("ctrl_{0}", sb);
        }
        private string _id;
        private int _width = 120;
        public string ElementId { get { return _id; } }
        public string Style { get; set; }
        public int Width { set { _width = value; } get { return _width; } }
    }
    public class CTRL_UserAutoComplete : CTRL_Base
    {
        public string SelectedCallback { get; set; }
        public string Clear()
        { 
            return string.Format("{0}.clear();", ElementId); 
        }
        public string GetValueCallback
        {
            get { return string.Format("{0}.value", ElementId); }
        }
    }

    public class CTRL_EnumDropDown : CTRL_Base
    {
        public string OptionLabel { get; set; }
        public List<NameValue> NameValueList { get; set; }
        public string SelectedCallback { get; set; }
        public string SelectedItemCallback { get; set; }
        public string Clear()
        {
            return string.Format("{0}.clear();", ElementId);
        }
        public string GetValueCallback
        {
            get { return string.Format("{0}.value", ElementId); }
        }
    }
}