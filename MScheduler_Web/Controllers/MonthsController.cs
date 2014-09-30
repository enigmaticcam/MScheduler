using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_Web.Models;

namespace MScheduler_Web.Controllers {
    public class MonthsController : BaseController {

        public ActionResult Index() {
            ViewState viewState = GetViewState();
            return View(viewState);
        }

        public ActionResult AddMonthWithOffset(int offset) {
            ViewState viewState = GetViewState();
            viewState.CurrentMonthSelectorView.CurrentMonth = viewState.CurrentMonthSelectorView.CurrentMonth.AddMonths(offset);
            return RedirectToAction("Index");
        }

        public ActionResult CreateMeetingConfirm(int dayOffset) {
            ViewState viewState = GetViewState();
            viewState.CurrentMonthSelectorView.CurrentDay = dayOffset;
            return View(viewState);
        }
	}
}