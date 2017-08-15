using System.ComponentModel;

namespace LFE.Core.Enums
{
    public class UserEnums
    {
        public enum eUserTypes
        {
             [Description("Normal user")] NormalUser = 1
            ,[Description("Super user")] SuperUser   = 2
        }
        

        public enum eUserStatuses
        {
             [Description("Active")] active     = 1
            ,[Description("Pending")] pending   = 2
            ,[Description("Disabled")] disabled = 3
            ,[Description("Locked")] locked     = 4
        }

        public enum eTrxTypes
        {
             [Description("All")] Unknow                      = 0
            ,[Description("DirectPayment")] DirectPayment     = 1
            ,[Description("ExpressCheckout")] ExpressCheckout = 2
            ,[Description("Complimentary")] Complimentary     = 3
            ,[Description("Seller payment")] Sellerpayment    = 4
            ,[Description("Free Coupon")] FreeCoupon          = 5
            ,[Description("Free Course")] FreeCourse          = 6
        }

        public enum eGenders
        {
            male = 1,
            female = 2
        }

        public enum eVideoActions
        {
            Play,
            Stop,
            Progress,
            Seek,
            Begin,
            Change,
            Complete
        }

        public enum eVideoActionsReasons
        {
            Begin         = 1,
            Stop          = 2,
            Complete      = 3,
            Seek          = 4,
            Change        = 5,
            WindowUnload  = 6,
            Blur          = 7,
            Focus         = 8,
            Ajax          = 9
        }
    }
}
