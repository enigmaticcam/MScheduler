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

        public MvcHtmlString DisplayTemplateListAsLinks() {
            ViewControlListAsLinks control = new ViewControlListAsLinks();
            control.TextIfThereAreNoLinks = "There are no templates";
            foreach (int templateId in _data.EditTemplateView.Templates.Keys) {
                control.AddLink(_data.EditTemplateView.Templates[templateId], "Action", "Control");
            }
            return _data.ViewBuilder.DisplayViewControlListAsLinks(control);
        }

        public void SetControllerContext(ControllerContext controllerContext, ViewDataDictionary viewDataDictionary, TempDataDictionary tempDataDictionary) {
            _data.ViewBuilder.SetControllerContext(controllerContext, viewDataDictionary, tempDataDictionary);
        }

        public ViewState(IFactory factory) {
            _data = new ViewStateData();
            _data.Factory = factory;
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