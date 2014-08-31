using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MScheduler_Web.Models {
    public interface IWebServer {
        void AddStatusMessage(TempDataDictionary tempData, string message);
        MvcHtmlString GetStatusMessage(TempDataDictionary tempData);
    }

    public class WebServer : IWebServer {
        public enum enumTempData {
            StatusMessage
        }

        public void AddStatusMessage(TempDataDictionary tempData, string message) {
            if (tempData.ContainsKey(enumTempData.StatusMessage.ToString())) {
                tempData[enumTempData.StatusMessage.ToString()] += "<br />" + message;
            } else {
                tempData.Add(enumTempData.StatusMessage.ToString(), message);
            }
        }

        public MvcHtmlString GetStatusMessage(TempDataDictionary tempData) {
            if (tempData.ContainsKey(enumTempData.StatusMessage.ToString())) {
                return new MvcHtmlString(tempData[enumTempData.StatusMessage.ToString()].ToString());
            } else {
                return new MvcHtmlString("");
            }
        }
    }

    public abstract class WebServerDecorator {
        private IWebServer _server;

        public virtual void AddStatusMessage(TempDataDictionary tempData, string message) {
            _server.AddStatusMessage(tempData, message);
        }

        public virtual MvcHtmlString GetStatusMessage(TempDataDictionary tempData) {
            return _server.GetStatusMessage(tempData);
        }

        public WebServerDecorator(IWebServer server) {
            _server = server;
        }
    }
}