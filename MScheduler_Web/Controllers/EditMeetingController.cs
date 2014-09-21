using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_BusTier.Concrete;
using MScheduler_Web.Models;

namespace MScheduler_Web.Controllers {
    public class EditMeetingController : BaseController {

        public ActionResult Meeting(int id) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.SetMeeting(id);
            return View(viewState);
        }

        public ActionResult CreateMeeting(EditMeetingView.CreateMeeting baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.BatonCreateMeeting = baton;
            return RedirectToAction("Meeting", new { id = viewState.CurrentEditMeetingView.Id });
        }
	}
}