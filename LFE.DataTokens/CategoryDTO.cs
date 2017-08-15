
namespace LFE.DataTokens
{
    public class CategoryDTO
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? OrderIndex { get; set; }
    }

    public class CategoryEditDTO 
    {
        public CategoryEditDTO()
        {
            id = -1;
            cnt = 0;
        }


        public int id { get; set; }

        public string name { get; set; }

        public bool isActive { get; set; }

        public int cnt { get; set; }
    }

    public class CategoryViewDTO
    {
        public int? id { get; set; }
        public string name { get; set; }
        public int? index { get; set; }
        public string url { get; set; }
    }
}
