using System.ComponentModel;

namespace LFE.Core.Enums
{
    public enum eSegmentTypes
    {
         Unknonw = 0
        ,[Description("My active learners")] Active = 1
        ,[Description("My non active learners")] InActive = 2
        ,[Description("Course or Bundle Segment")]Item = 3
        ,[Description("Course or Bundle Segment For New Subscribers")]ItemNew = 4
    }

}
