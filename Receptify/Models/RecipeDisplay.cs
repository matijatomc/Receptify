using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receptify.Models
{
    public class RecipeDisplay
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CookingTime { get; set; }
        public List<string> Tags { get; set; } = new();
        public string TagList => Tags != null && Tags.Count > 0 ? string.Join(", ", Tags) : "Bez oznaka";
    }
}
