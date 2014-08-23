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
        protected IFactory DefaultFactory {
            get {
                if (_defaultFactory == null) {
                    _defaultFactory = Factory.CreateInstance();
                }
                return _defaultFactory;
            }
        }

        private IWebServer _defaultServer;
        protected IWebServer DefaultServer {
            get {
                if (_defaultServer == null) {
                    _defaultServer = new WebServer();
                }
                return _defaultServer;
            }
        }
	}
}