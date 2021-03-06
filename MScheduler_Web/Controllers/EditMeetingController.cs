﻿using System;
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
            viewState.CurrentEditMeetingView.PerformValidations();
            string message = viewState.CurrentEditMeetingView.Message;
            if (message.Length > 0) {
                this.DefaultServer.AddStatusMessage(TempData, message);
            }
            return View(viewState);
        }

        public ActionResult CreateMeeting(EditMeetingView.CreateMeeting baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.BatonCreateMeeting = baton;
            return RedirectToAction("Meeting", new { id = viewState.CurrentEditMeetingView.Id });
        }

        public ActionResult EditMeeting(BatonMeeting baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.BatonMeeting = baton.Export();
            return RedirectToAction("Meeting", new { id = baton.Id });
        }

        public ActionResult Save(int id) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.Save();
            return RedirectToAction("Meeting", new { id = id });
        }

        public ActionResult CreateSlot(BatonCreateMeetingSlot baton) {
            ViewState viewState = GetViewState();
            viewState.CurrentEditMeetingView.BatonCreateSlot = baton.Export();
            return RedirectToAction("Meeting", new { id = baton.MeetingId });
        }

        public ActionResult RefreshWithId(int id) {
            GetViewState(true);
            return RedirectToAction("Meeting", new { id = id });
        }
	}
}