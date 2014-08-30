using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MScheduler_Web.Models {
    public class ViewControlListAsLinks {
        public List<string> LinksDisplayName;
        public List<string> LinksActionName;
        public List<string> LinksControlName;
        public string TextIfThereAreNoLinks;

        public void AddLink(string displayName, string actionName, string controlName) {
            this.LinksDisplayName.Add(displayName);
            this.LinksActionName.Add(actionName);
            this.LinksControlName.Add(controlName);            
        }

        public ViewControlListAsLinks() {
            this.LinksDisplayName = new List<string>();
            this.LinksActionName = new List<string>();
            this.LinksControlName = new List<string>();
        }
    }
}