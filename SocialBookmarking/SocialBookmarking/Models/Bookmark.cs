using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialBookmarking.Models
{
    public class Bookmark
    {
        [Key] 
        public int BookmarkId { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [MaxLength(100, ErrorMessage = "Titlul este prea lung")]
        public string BookmarkTitle { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string BookmarkDesc { get; set; }
        public DateTime BookmarkDate { get; set; }

        public string BookmarkTags { get; set; }
        public int BookmarkPopularity { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")] // ! nici aici nu stiu daca e bine
        public int CategoryId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public IEnumerable<SelectListItem> Categ { get; set; }
    }
}