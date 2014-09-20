using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_BusTier.Concrete;
using MScheduler_Web.Models;

namespace MScheduler_Web.Controllers {
    public class EditMeetingController : BaseController {
        
        public ActionResult Index() {
            ViewState viewState = GetViewState();
            return View(viewState);
        }

        public ActionResult Meeting(int id) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.SetMeeting(id);
            return View(viewState);
        }

        public ActionResult AddMonthWithOffset(int offset) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.CurrentMonth = viewState.CurrentEditMeetingView.CurrentMonth.AddMonths(offset);
            return RedirectToAction("Index");
        }

        public ActionResult CreateMeetingConfirm(int dayOffset) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.CurrentDay = dayOffset;
            return View(viewState);
        }

        public ActionResult CreateMeeting(EditMeetingView.CreateMeeting baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.BatonCreateMeeting = baton;
            return RedirectToAction("Meeting", new { id = viewState.CurrentEditMeetingView.Id });
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