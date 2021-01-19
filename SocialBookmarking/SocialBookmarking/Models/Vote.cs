using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SocialBookmarking.Models
{
    public class Vote
    {
        [Key]
        [Column(Order = 0)]
        public int BookmarkId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string UserId { get; set; }

    }
}