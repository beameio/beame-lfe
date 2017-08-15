using LFE.DataTokens;
using LFE.Portal.Areas.Widget.Models;

namespace LFE.Portal.Areas.Widget.Helpers
{
    public static class ItemExtensions
    {
        public static ItemIntroToken ProductPageToken2ItemIntroToken(this ItemProductPageToken token)
        {
            return new ItemIntroToken
                                    {
                                        ItemId                   = token.ItemId
                                        ,ItemType                = token.ItemType
                                        ,ItemName                = token.ItemName
                                        ,IntroHtml               = token.IntroHtml
                                        ,OverviewVideoIdentifier = token.OverviewVideoIdentifier
                                        ,VideoInfoToken          = token.VideoInfoToken
                                        ,TrackingID              = token.TrackingID
    
                                    };
        }

        public static ItemIntroToken ViewerPageToken2ItemIntroToken(this ItemViewerPageToken token)
        {
            return new ItemIntroToken
                                    {
                                        ItemId                   = token.ItemId
                                        ,ItemType                = token.ItemType
                                        ,ItemName                = token.ItemName
                                        ,IntroHtml               = token.IntroHtml
                                        ,OverviewVideoIdentifier = token.OverviewVideoIdentifier
                                        ,VideoInfoToken          = token.VideoInfoToken
                                        ,TrackingID              = token.TrackingID
    
                                    };
        }

        public static string Sort2DisplayName(this string sort)
        {
            switch (sort)
            {
                case "ordinal":
                    return "Name";
                case "name":
                    return "Name";
                case "popularity":
                    return "Popularity";
                case "cost":
                    return "Price";
                case "rating":
                    return "Ranking";
                default:
                    return "Name";
            }
        }
    }
}