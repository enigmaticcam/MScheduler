﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_Web.Models;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;

namespace MScheduler_Web.Controllers {
    public class EditTemplateController : BaseController {
        
        public ActionResult Index() {
            ViewState viewState = GetViewState();
            return View(viewState);
        }

        public ActionResult CreateTemplate() {
            ViewState viewState = GetViewState();
            int templateId = viewState.CurrentEditTemplateView.CreateTemplate();
            if (templateId == 0) {
                this.DefaultServer.AddStatusMessage(TempData, "Could not create tempalte");
                return RedirectToAction("Index");
            } else {
                return RedirectToAction("Template", new { id = templateId });
            }
        }

        public ActionResult Template(int id) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditTemplateView.SetTemplate(id);
            if (viewState.CurrentEditTemplateView.Message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, viewState.CurrentEditTemplateView.Message);
                return RedirectToAction("Index");
            } else {
                return View(viewState);
            }
        }

        public ActionResult EditTemplate(EditTemplateView.Baton baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditTemplateView.DataBaton = baton;
            if (viewState.CurrentEditTemplateView.Message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, viewState.CurrentEditTemplateView.Message);                
            }
            return RedirectToAction("Template", new { id = baton.Id });
        }

        public ActionResult Save(int id) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditTemplateView.Save();
            if (viewState.CurrentEditTemplateView.Message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, viewState.CurrentEditTemplateView.Message);
            }
            return RedirectToAction("Template", new { id = id });
        }

        public ActionResult Delete(int id) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditTemplateView.Delete(id);
            if (viewState.CurrentEditTemplateView.Message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, viewState.CurrentEditTemplateView.Message);
            }
            return RedirectToAction("Template", new { id = id });
        }

        public ActionResult RefreshWithId(int id) {
            ViewState viewState = GetViewState(true);
            return RedirectToAction("Template", new { id = id });
        }

        public ActionResult Refresh() {
            ViewState viewState = GetViewState(true);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [MultipleButton(Name = "TemplateSlotTable", Argument = "AddSlot")]
        public ActionResult AddSlot(BatonTemplateSlots baton) {
            ViewState viewState = GetViewState();
            if (baton.TemplateSlots != null) {
                viewState.CurrentEditTemplateView.NewSlot(baton.TemplateId, baton.Export());
            } else {
                viewState.CurrentEditTemplateView.NewSlot(baton.TemplateId, null);
            }
            if (viewState.CurrentEditTemplateView.Message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, viewState.CurrentEditTemplateView.Message);
            }
            return RedirectToAction("Template", new { id = baton.TemplateId });
        }

        [HttpPost]
        [MultipleButton(Name = "TemplateSlotTable", Argument = "Update")]
        public ActionResult Update(BatonTemplateSlots baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditTemplateView.Slots = baton.Export();
            if (viewState.CurrentEditTemplateView.Message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, viewState.CurrentEditTemplateView.Message);
            }
            return RedirectToAction("Template", new { id = baton.TemplateId });
        }
	}
}