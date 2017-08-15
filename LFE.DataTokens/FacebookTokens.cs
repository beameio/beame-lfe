namespace LFE.DataTokens
{
    public class FacebookAppUserToken
    {
        public string Id { get; set; }
        public string Name { get; set; }
        //public string Email { get; set; }
        //public FacebookPage Page { get; set; }
       // public FacebookConnection<FacebookPage> Page { get; set; }
        
        //[JsonProperty("picture")] // This renames the property to picture.
        //// [FacebookFieldModifier("type(large)")] // This sets the picture size to large.
        //public FacebookConnection<FacebookPicture> ProfilePicture { get; set; }

        ////[FacebookFieldModifier("limit(8)")] // This sets the size of the friend list to 8, remove it to get all friends.
        //public FacebookGroupConnection<MyAppUserFriendSimple> Friends { get; set; }

        //[FacebookFieldModifier("limit(16)")] // This sets the size of the photo list to 16, remove it to get all photos.
        //public FacebookGroupConnection<FacebookPhoto> Photos { get; set; }
    }

    public class FacebookPageToken
    {
        public string TrackingId { get; set; }
        public bool Liked { get; set; }
        public bool Admin { get; set; }
        public string AccessToken { get; set; }
        public long? ProviderUid { get; set; }
        public ItemProductPageToken ItemProductPageToken { get; set; }
        public IndexModelViewToken IndexModelViewToken { get; set; }
    }




    public class FbSignedRequestToken
    {
        public string algorithm { get; set; }
        public int issued_at { get; set; }
        public Page page { get; set; }
        public User user { get; set; }
    }

    public class Page
    {
        public string id { get; set; }
        public bool liked { get; set; }
        public bool admin { get; set; }
    }

    public class Age
    {
        public int min { get; set; }
    }

    public class User
    {
        public string country { get; set; }
        public string locale { get; set; }
        public Age age { get; set; }
    }
}
