using Microsoft.AspNet.Identity;
using SocialBookmarking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SocialBookmarking.Controllers
{
    public class CommentsController : Controller
    {
        // GET: Comments
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult Index()
        {
            return View();
        }

        [HttpDelete]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Bookmarks");
            }
        }
        /*
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public ActionResult New(Comment comm)
        {
            comm.CommentDate = DateTime.Now;
            try
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }

            catch (Exception e)
            {
                return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
            }

        }*/

        [Authorize(Roles = "User, Administrator")]
        public ActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);
            if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return RedirectToAction("Index", "Bookmarks");
            }
        }

        [HttpPut]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Edit(int id, Comment requestComment)
        {
            /*try
            {
                Comment comm = db.Comments.Find(id);
                if (TryUpdateModel(comm))
                {
                    category.CategoryName = requestComment.CategoryName;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View(requestComment);
            }*/

            try
            {
                Comment comm = db.Comments.Find(id);
                
                if (comm.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                {
                    if (TryUpdateModel(comm))
                    {
                        comm.CommentContent = requestComment.CommentContent;
                        db.SaveChanges();
                    }
                    return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                    return RedirectToAction("Index", "Bookmarks");
                }
                
            }
            catch (Exception e)
            {
                return View(requestComment);
            }

        }

    }
}