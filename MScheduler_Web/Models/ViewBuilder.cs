using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MScheduler_Web.Models {
    public class ViewBuilder {
        private ControllerContext _controllerContext;
        private ViewDataDictionary _viewDataDictionary;
        private TempDataDictionary _tempDataDictionary;

        private MvcHtmlString RenderViewToString(string viewName) {
            return RenderViewToString(viewName, null);
        }

        private MvcHtmlString RenderViewToString(string viewName, object model) {
            _viewDataDictionary.Model = model;
            StringWriter sw = new StringWriter();
            ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(_controllerContext, viewName);
            ViewContext viewContext = new ViewContext(_controllerContext, viewResult.View, _viewDataDictionary, _tempDataDictionary, sw);
            viewResult.View.Render(viewContext, sw);
            viewResult.ViewEngine.ReleaseView(_controllerContext, viewResult.View);
            return new MvcHtmlString(sw.GetStringBuilder().ToString());
        }

        public void SetControllerContext(ControllerContext controllerContext, ViewDataDictionary viewDataDictionary, TempDataDictionary tempDataDictionary) {
            _controllerContext = controllerContext;
            _viewDataDictionary = viewDataDictionary;
            _tempDataDictionary = tempDataDictionary;
        }
    }
}