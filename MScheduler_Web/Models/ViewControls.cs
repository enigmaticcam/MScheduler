using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;

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

    public class BatonTemplateSlots {
        public List<SelectListItem> SlotTypes { get; set; }
        public int TemplateId { get; set; }

        public List<BatonTemplateSlot> TemplateSlots { get; set; }
        public List<TemplateSlot> Export() {
            return 
                (from n in this.TemplateSlots
                select n.Export()).ToList();
        }
    }

    public class BatonTemplateSlot {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int SlotTypeId { get; set; }
        public string Title { get; set; }
        public int SortNumber { get; set; }
        public List<SelectListItem> SlotTypes { get; set; }

        public void Import(TemplateSlot templateSlot) {
            this.Id = templateSlot.Id;
            this.TemplateId = templateSlot.Id;
            this.SlotTypeId = (int)templateSlot.SlotType;
            this.Title = templateSlot.Title;
            this.SortNumber = templateSlot.SortNumber;            
        }

        public TemplateSlot Export() {
            TemplateSlot slot = new TemplateSlot();
            slot.Id = this.Id;
            slot.SlotType = (Slot.enumSlotType)this.SlotTypeId;
            slot.Title = this.Title;
            slot.SortNumber = this.SortNumber;
            slot.TemplateId = this.TemplateId;
            return slot;
        }
    }
}