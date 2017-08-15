using System.ComponentModel;

namespace LFE.Core.Enums
{
    public enum eMessageKinds
    {
         Unknown
        ,[Description("Event")] EVENT = 1
        ,[Description("Author Message")] AUTHOR_MESSAGE = 2
        ,[Description("User Message")] USER_MESSAGE = 3
    }

    public enum eFeedFilterKinds
    {
        User,
        Hashtag
    }

    public enum DiscussionSortFields
    {
        AddOn
        ,CreatorName
        , CourseName
    }
}
