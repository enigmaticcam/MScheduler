using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_Web.Models;

namespace MScheduler_Web.Controllers {
    public class EditMeetingController : BaseController {
        
        public ActionResult Index() {
            ViewState viewState = GetViewState();
            return View(viewState);
        }

        public ActionResult AddMonthWithOffset(int offset) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.CurrentMonth = viewState.CurrentEditMeetingView.CurrentMonth.AddMonths(offset);
            return RedirectToAction("Index");
        }

        public ActionResult CreateMeetingConfirm() {
            ViewState viewState = GetViewState();
            return View(viewState);
        }

        private ViewState GetViewState(bool refresh = false) {
            ViewState viewState = (ViewState)Session["ViewState"];
            if (viewState == null || refresh) {
                viewState = new ViewState(this.DefaultFactory, this.DefaultServer);
                Session["ViewState"] = viewState;
            }
            viewState.SetControllerContext(ControllerContext, ViewData, TempData);
            return viewState;
        }
	}
}