using Microsoft.AspNet.Identity;
using SocialBookmarking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialBookmarking.Controllers
{
    public class SavesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int _perPage = 3;
        // GET: Saves
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            var saves = db.Saves.Include("Bookmark").Where(s => s.UserId.Equals(userId)).OrderByDescending(s => s.SaveDate);
            
            var totalItems = saves.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedSaves = saves.Skip(offset).Take(this._perPage);

            ViewBag.Saves = saves;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Saves = paginatedSaves;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult New(Save save)
        {
            save.UserId = User.Identity.GetUserId();
            save.SaveDate = DateTime.Now;
            try
            {
                db.Saves.Add(save);
                db.SaveChanges();
                //UpdatePopularityP(vote.BookmarkId);
                return Redirect("/Bookmarks/Show/" + save.BookmarkID);
            }

            catch (Exception e)
            {
                return Redirect("/Bookmarks/Show/" + save.BookmarkID);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Delete(int bId)
        {
            Save save = db.Saves.Find(bId, User.Identity.GetUserId());
            if (save != null)
            {
                db.Saves.Remove(save);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + save.BookmarkID);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti aceasta modificare";
                return RedirectToAction("Index", "Bookmarks");
            }
        }
    }
}