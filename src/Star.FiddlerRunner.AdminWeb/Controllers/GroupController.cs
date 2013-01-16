using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Star.FiddlerRunner.Common;

namespace Star.FiddlerRunner.AdminWeb.Controllers
{
    public class GroupController : Controller
    {
        readonly ISessionRepository _repo = new MongoSessionRepository();
        //
        // GET: /Group/

        public ActionResult Index()
        {

            return View();
        }

        //
        // GET: /Group/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Group/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Group/Create

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
        // GET: /Group/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Group/Edit/5

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
        // GET: /Group/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Group/Delete/5

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

        public ActionResult Sessions(string id)
        {
            var list = _repo.GetSessionListForGroupId(id);

            return View(list);
        }
    }
}
