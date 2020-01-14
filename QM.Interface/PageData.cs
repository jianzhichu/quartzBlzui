using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace QM.Interface
{
    public class PageInfo
    {

        public static PageInfo defaultPage = new PageInfo();
        [JsonProperty(Order = 1)]
        public int PageIndex { get; set; } = 1;
        [JsonProperty(Order = 2)]
        public int PageSize { get; set; } = 10;


    }


    public class PageData<T>
    {
        public long Total { get; set; }
        public IEnumerable<T> Rows { get; set; }
    }

    public enum Order
    {
        Ascending,
        Descending
    }
}
