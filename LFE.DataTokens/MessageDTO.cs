using System;

namespace LFE.DataTokens
{
    public class ReviewMessageDTO
    {
        public DateTime AddOn { get; set; }
        public string ReviewText { get; set; }
        public ItemMessageDTO Item { get; set; }
        public MessageUserDTO Writer { get; set; }
        public MessageUserDTO Author { get; set; }
        public MessageUserDTO Learner { get; set; }
    }

    public class PurchaseMessageDTO
    {
        public DateTime AddOn { get; set; }
        public string ReviewText { get; set; }
        public ItemMessageDTO Item { get; set; }
        public MessageUserDTO Buyer { get; set; }
        public MessageUserDTO Author { get; set; }        
    }

    public class ItemMessageDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string itemUrlName { get; set; }
        public string desc { get; set; }
        public string thumbUrl { get; set; }
    }

    public class MessageUserDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public long? fbUid { get; set; }
    }
}
