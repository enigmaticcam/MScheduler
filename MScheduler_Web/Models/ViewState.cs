using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;

namespace MScheduler_Web.Models {
    public class ViewState {
        private ViewStateData _data;
        private IWebServer _webServer;

        public IEditTemplateView CurrentEditTemplateView {
            get { return _data.EditTemplateView; }
        }

        public MvcHtmlString StatusMessage() {
            return _webServer.GetStatusMessage(_data.ViewBuilder.TempDataDictionary);
        }

        public MvcHtmlString DisplayTemplateListAsLinks() {
            ViewControlListAsLinks control = new ViewControlListAsLinks();
            control.TextIfThereAreNoLinks = "There are no templates";
            foreach (int templateId in _data.EditTemplateView.Templates.Keys) {
                control.AddLink(_data.EditTemplateView.Templates[templateId], "Template", "EditTemplate", new { id = templateId });
            }
            return _data.ViewBuilder.DisplayViewControlListAsLinks(control);
        }

        public MvcHtmlString DisplayTemplateProperties() {
            return _data.ViewBuilder.DisplayTemplateProperties(_data.EditTemplateView.DataBaton);
        }

        public void SetControllerContext(ControllerContext controllerContext, ViewDataDictionary viewDataDictionary, TempDataDictionary tempDataDictionary) {
            _data.ViewBuilder.SetControllerContext(controllerContext, viewDataDictionary, tempDataDictionary);
        }

        public ViewState(IFactory factory, IWebServer webServer) {
            _data = new ViewStateData();
            _data.Factory = factory;
            _webServer = webServer;
        }

        public class ViewStateData {
            public IFactory Factory { get; set; }

            private ViewBuilder _viewBuilder;
            public ViewBuilder ViewBuilder {
                get {
                    if (_viewBuilder == null)
                        _viewBuilder = new ViewBuilder();
                    return _viewBuilder;
                }
            }

            private IEditTemplateView _editTemplateView;
            public IEditTemplateView EditTemplateView {
                get {
                    if (_editTemplateView == null) {
                        _editTemplateView = this.Factory.CreateEditTemplateView();
                        _editTemplateView.LoadFromSource();
                    }
                    return _editTemplateView;
                }
            }
        }
    }

    
}