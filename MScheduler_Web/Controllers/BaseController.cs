using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_BusTier.Abstract;
using MScheduler_Web.Models;

namespace MScheduler_Web.Controllers {
    public class BaseController : Controller {
        private IFactory _defaultFactory;
        public IFactory DefaultFactory {
            get {
                if (_defaultFactory == null) {
                    _defaultFactory = Factory.CreateInstance(Factory.enumDatabaseInstance.Development);
                }
                return _defaultFactory;
            }
        }

        private IWebServer _defaultServer;
        public IWebServer DefaultServer {
            get {
                if (_defaultServer == null) {
                    _defaultServer = new WebServer();
                }
                return _defaultServer;
            }
        }

        public ViewState GetViewState(bool refresh = false) {
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