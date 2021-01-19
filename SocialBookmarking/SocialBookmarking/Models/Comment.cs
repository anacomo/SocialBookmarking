using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SocialBookmarking.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Comentariul nu poate fi gol")]
        [MaxLength(500, ErrorMessage = "Categoria este prea lunga")]
        public string CommentContent { get; set; }
        public DateTime CommentDate { get; set; }
        public int BookmarkId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual Bookmark Bookmark { get; set; }

    }
}