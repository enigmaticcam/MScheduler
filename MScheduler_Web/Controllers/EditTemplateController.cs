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