using Microsoft.AspNet.Identity;
using SocialBookmarking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialBookmarking.Controllers
{
    public class VotesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Votes
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult New(Vote vote)
        {
            vote.UserId = User.Identity.GetUserId();
            try
            {
                db.Votes.Add(vote);
                db.SaveChanges();
                UpdatePopularityP(vote.BookmarkId);
                return Redirect("/Bookmarks/Show/" + vote.BookmarkId);
            }

            catch (Exception e)
            {
                return Redirect("/Bookmarks/Show/" + vote.BookmarkId);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Delete(int bId)
        {
            Vote vote = db.Votes.Find(bId, User.Identity.GetUserId());
            if (vote != null)
            {
                db.Votes.Remove(vote);
                db.SaveChanges();
                UpdatePopularityM(vote.BookmarkId);
                return Redirect("/Bookmarks/Show/" + vote.BookmarkId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti aceasta modificare";
                return RedirectToAction("Index", "Bookmarks");
            }
        }

        private void UpdatePopularityP(int id)
        {
           
            if (ModelState.IsValid)
            {
                Bookmark bookmark = db.Bookmarks.Find(id);
                if (TryUpdateModel(bookmark))
                {
                    bookmark.BookmarkPopularity = bookmark.BookmarkPopularity + 1;
                    db.SaveChanges();
                }
            }
        }
        private void UpdatePopularityM(int id)
        {

            if (ModelState.IsValid)
            {
                Bookmark bookmark = db.Bookmarks.Find(id);
                if (TryUpdateModel(bookmark))
                {
                    if (bookmark.BookmarkPopularity > 0)
                    {
                        bookmark.BookmarkPopularity = bookmark.BookmarkPopularity - 1;
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}