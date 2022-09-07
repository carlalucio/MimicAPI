using MimicAPI.V1.Models.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace MimicAPI.Helpers
{
    public class PaginationList<T>
    {
        
        public List<T> Results { get; set; } = new List<T>();
        [JsonIgnore]
        public Paginacao Paginacao { get; set; }        
        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();
    }
}
