using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SocialBookmarking.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        [MaxLength(50, ErrorMessage = "Categoria este prea lunga")]
        public string CategoryName { get; set; }
        public virtual ICollection<Bookmark> Bookmarks { get; set; }
    }
}