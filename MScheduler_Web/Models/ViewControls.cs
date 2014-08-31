using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MScheduler_Web.Models {
    public class ViewControlListAsLinks {
        public List<string> LinksDisplayName { get; set; }
        public List<string> LinksActionName { get; set; }
        public List<string> LinksControlName { get; set; }
        public List<object> LinksModel { get; set; }
        public string TextIfThereAreNoLinks { get; set; }

        public void AddLink(string displayName, string actionName, string controlName, object model) {
            this.LinksDisplayName.Add(displayName);
            this.LinksActionName.Add(actionName);
            this.LinksControlName.Add(controlName);
            this.LinksModel.Add(model);
        }

        public ViewControlListAsLinks() {
            this.LinksDisplayName = new List<string>();
            this.LinksActionName = new List<string>();
            this.LinksControlName = new List<string>();
            this.LinksModel = new List<object>();
        }
    }

    public class ViewControlDropDownList {
        public string Text { get; set; }
        public string HtmlId { get; set; }
        public string TextIfThereAreNoItems { get; set; }
        public List<SelectListItem> Items { get; set; }
    }
}