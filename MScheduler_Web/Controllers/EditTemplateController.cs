using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_Web.Models;

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
                return RedirectToAction("Index");
            }
        }

        private ViewState GetViewState(bool refresh = false) {
            ViewState viewState = (ViewState)Session["ViewState"];
            if (viewState == null || refresh) {
                viewState = new ViewState(this.DefaultFactory);
                Session["ViewState"] = viewState;
            }
            viewState.SetControllerContext(ControllerContext, ViewData, TempData);
            return viewState;
        }
	}
}