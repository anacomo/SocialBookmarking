using Microsoft.AspNet.Identity;
using SocialBookmarking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace SocialBookmarking.Controllers
{
    public class BookmarksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int _perPage = 3;

        // GET: Bookmarks
        public ActionResult Index()
        {
            var bookmarks = db.Bookmarks.Include("Category").Include("User").OrderByDescending(b => b.BookmarkPopularity).OrderByDescending(a => a.BookmarkDate);
            var search = "";
            var sort = Request.Params.Get("sort");

            if (Request.Params.Get("search") != null)
            {
                search = Request.Params.Get("search").Trim(); // trim whitespace from search string
                // search in bookmarks (title and content)
                List<int> bookmarkIds = db.Bookmarks.Where(
                    at => at.BookmarkTitle.Contains(search)
                    || at.BookmarkDesc.Contains(search)
                    || at.BookmarkTags.Contains(search)
                    ).Select(a => a.BookmarkId).ToList();
                // search in comments (content)
                List<int> commentIds = db.Comments.Where(c => c.CommentContent.Contains(search))
                    .Select(com => com.BookmarkId).ToList();

                // unique list of bookmarks
                List<int> mergedIds = bookmarkIds.Union(commentIds).ToList();

                // list of articles that contain the seatch string either in article title, content or comments
                bookmarks = db.Bookmarks.Where(bookmark => mergedIds.Contains(bookmark.BookmarkId)).Include("Category").Include("User").OrderBy(a => a.BookmarkDate);
            }
            if (sort == "Cele mai recente")
            {
                bookmarks = bookmarks.OrderByDescending(b => b.BookmarkPopularity).OrderByDescending(a => a.BookmarkDate);
            }
            else
            {
               bookmarks = bookmarks.OrderByDescending(a => a.BookmarkDate).OrderByDescending(b => b.BookmarkPopularity);
            }


            var totalItems = bookmarks.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * this._perPage;
            }

            var paginatedBookmarks = bookmarks.Skip(offset).Take(this._perPage);

            ViewBag.Bookmarks = bookmarks;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.total = totalItems;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Bookmarks = paginatedBookmarks;
            ViewBag.SearchString = search;
            //ViewBag.search = search;
            ViewBag.Sort = sort;
            //ViewBag.sort = sort;
            return View();
        }

        public ActionResult Show(int id)
        {
            Bookmark bookmark = db.Bookmarks.Find(id);
            SetAccessRights();
            
            Vote vote = db.Votes.Find(id, User.Identity.GetUserId());
            if (vote == null)
                ViewBag.voted = false;
            else
                ViewBag.voted = true;

            Save save = db.Saves.Find(id, User.Identity.GetUserId());
            if (save == null)
                ViewBag.saved = false;
            else
                ViewBag.saved = true;

            return View(bookmark);
        }

        [HttpPost]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Show(Comment comm)
        {
            comm.CommentDate = DateTime.Now;
            comm.UserId = User.Identity.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    db.Comments.Add(comm);
                    db.SaveChanges();

                    // get the article for which the comment was added
                    // comm este o instanta primita de la view care inca nu are legatura cu articolol pentru care a fost adaugat
                    Bookmark commBookmark = db.Bookmarks.Find(comm.BookmarkId);
                    string authorEmail = comm.Bookmark.User.Email;

                    // create the notification body
                    string notificationBody = "<p>A fost adaugat un nou comentariu la articolul dvs cu titlul: </p>";
                    notificationBody += "<p><strong>" + commBookmark.BookmarkTitle + "</strong></p>";
                    notificationBody += "<br />";
                    notificationBody += "Comentariul este adaugat de catre : <br />";
                    notificationBody += "<em>" + comm.CommentContent + "</em>";
                    notificationBody += "<br /><br /> Va dorim o zi frumoasa!";

                    SendEmailNotification(authorEmail, "Un nou comentariu a fost adaugat la articolul dvs.", notificationBody);

                    return Redirect("/Bookmarks/Show/" + comm.BookmarkId);
                }

                else
                {
                    Bookmark b = db.Bookmarks.Find(comm.BookmarkId);
                    SetAccessRights();
                    return View(b);
                }

            }

            catch (Exception e)
            {
                Bookmark b = db.Bookmarks.Find(comm.BookmarkId);
                SetAccessRights();
                return View(b);
            }

        }

        [Authorize(Roles = "User, Administrator")]
        public ActionResult New()
        {
            Bookmark bookmark = new Bookmark();
            // preluam lista de categorii din metoda GetAllCategories()
            bookmark.Categ = GetAllCategories();

            //Preluam ID-ul utilizatorului curent
            bookmark.UserId = User.Identity.GetUserId();
            return View(bookmark);
        }

        [HttpPost]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult New(Bookmark bookmark)
        { 
            bookmark.BookmarkDate = DateTime.Now;
            bookmark.UserId = User.Identity.GetUserId();
            try
            {
                if(ModelState.IsValid)
                {
                    db.Bookmarks.Add(bookmark);
                    db.SaveChanges();
                    TempData["message"] = "Bookmarkul a fost adaugat!";
                    return RedirectToAction("Index");
                }
                else
                {
                    bookmark.Categ = GetAllCategories();
                    return View(bookmark);
                }
            }
            catch (Exception e)
            {
                return View(bookmark);
            }
        }

        [Authorize(Roles = "User, Administrator")]
        public ActionResult Edit(int id)
        {
            Bookmark bookmark = db.Bookmarks.Find(id);
            bookmark.Categ = GetAllCategories();
            if (bookmark.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(bookmark);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa modificati.";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Edit(int id, Bookmark requestBookmark)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    Bookmark bookmark = db.Bookmarks.Find(id);
                    if (bookmark.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(bookmark))
                        {
                            bookmark.BookmarkTitle = requestBookmark.BookmarkTitle;
                            bookmark.BookmarkDesc = requestBookmark.BookmarkDesc;
                            bookmark.BookmarkDate = requestBookmark.BookmarkDate;
                            bookmark.CategoryId = requestBookmark.CategoryId;
                            db.SaveChanges();
                            TempData["message"] = "Bookmarkul a fost modificat!";
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bookmark care nu va apartine";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    requestBookmark.Categ = GetAllCategories();
                    return View(requestBookmark);
                }
            }
            catch (Exception e)
            {
                requestBookmark.Categ = GetAllCategories();
                return View(requestBookmark);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User, Administrator")]
        public ActionResult Delete(int id)
        {
            Bookmark bookmark = db.Bookmarks.Find(id);
            if (bookmark.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                db.Bookmarks.Remove(bookmark);
                db.SaveChanges();
                TempData["message"] = "Bookmarkul a fost sters!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un bookmark care nu va apartine";
                return RedirectToAction("Index");
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();
            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;
            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            // returnam lista de categorii
            return selectList;
        }

        private void SetAccessRights()
        {
            ViewBag.afisareButoane = false;
            if (User.IsInRole("User") || User.IsInRole("Administrator"))
            {
                ViewBag.afisareButoane = true;
            }

            ViewBag.esteAdmin = User.IsInRole("Administrator");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
        }

        private void SendEmailNotification(string toEmail, string subject, string content)
        {
            const string senderEmail = "testemaildaw@gmail.com";
            const string senderPassword = "parola";
            const string smtpServer = "smtp.gmail.com";
            const int smtpPort = 587;

            // create a new smto client that is used to send emails
            SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            // create a new email object
            MailMessage email = new MailMessage(senderEmail, toEmail, subject, content);
            email.IsBodyHtml = true;
            email.BodyEncoding = UTF8Encoding.UTF8;

            try
            {
                // to send the mail
                System.Diagnostics.Debug.WriteLine("Sending mail ...");
                smtpClient.Send(email);
                System.Diagnostics.Debug.WriteLine("Email sent!");
            }
            catch(Exception e)
            {
                // failed to send the message
                System.Diagnostics.Debug.WriteLine("Error occured while trying to send the e-mail.");
                System.Diagnostics.Debug.WriteLine(e.Message.ToString());
            }
        }
    }
}