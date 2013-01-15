using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Star.FiddlerRunner.Common;

namespace Star.FiddlerRunner.AdminWeb.Controllers
{
    public class SequenceController : Controller
    {
        //
        // GET: /Sequence/

        public ActionResult Index()
        {
            Star.FiddlerRunner.Common.ISessionRepository repo = new MongoSessionRepository();

            var list = repo.GetSessionSequenceList();
            return View(list);
        }

        //
        // GET: /Sequence/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Sequence/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Sequence/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Sequence/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Sequence/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Sequence/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Sequence/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
