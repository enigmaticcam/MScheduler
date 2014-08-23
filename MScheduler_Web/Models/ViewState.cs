using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_BusTier.Abstract;

namespace MScheduler_Web.Models {
    public class ViewState {
        private ViewStateData _data;

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
        }
    }

    
}