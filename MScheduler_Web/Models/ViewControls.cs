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
        public IEnumerable<SelectListItem> SlotTypes {
            get {
                foreach (Slot.enumSlotType slotType in Enum.GetValues(typeof(Slot.enumSlotType))) {
                    if (slotType != Slot.enumSlotType.None) {
                        SelectListItem item = new SelectListItem();
                        item.Text = slotType.ToString();
                        item.Value = ((int)slotType).ToString();
                        if ((Slot.enumSlotType)this.SlotTypeId == slotType) {
                            item.Selected = true;
                        }
                        yield return item;
                    }                    
                }
            }
        }

        public void Import(TemplateSlot templateSlot) {
            this.Id = templateSlot.Id;
            this.TemplateId = templateSlot.TemplateId;
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

    public class BatonSlots {
        public int MeetingId { get; set; }

        public List<BatonSlot> Slots { get; set; }
        public List<EditMeetingView.BatonSlot> Export() {
            return
                (from n in this.Slots
                 select n.Export()).ToList();
        }

    }

    public class BatonSlot {
        public int SlotId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SlotFillerId { get; set; }
        public int SortNumber { get; set; }
        public string SlotType { get; set; }
        public List<SelectListItem> SlotFillers { get; set; }

        public void Import(EditMeetingView.BatonSlot batonSlot) {
            this.SlotId = batonSlot.SlotId;
            this.Title = batonSlot.Title;
            this.Description = batonSlot.Description;
            this.SortNumber = batonSlot.SortNumber;
            this.SlotType = batonSlot.SlotType;
            this.SlotFillers = new List<SelectListItem>();
            foreach (SelectionItem item in batonSlot.SlotFillers) {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Text = item.Text;
                selectItem.Value = item.Value;
                selectItem.Selected = item.IsSelected;
                this.SlotFillers.Add(selectItem);
            }
        }

        public EditMeetingView.BatonSlot Export() {
            EditMeetingView.BatonSlot batonSlot = new EditMeetingView.BatonSlot();
            batonSlot.SlotId = this.SlotId;
            batonSlot.Title = this.Title;
            batonSlot.Description = this.Description;
            batonSlot.SortNumber = this.SortNumber;
            batonSlot.SlotType = this.SlotType;
            return batonSlot;
        }
    }

    public class BatonCreateMeeting {
        public List<SelectListItem> Templates { get; set; }
        public int TemplateId { get; set; }
        public bool UseTemplate { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public void Import(EditMeetingView.CreateMeeting baton) {
            this.TemplateId = baton.TemplateId;
            this.UseTemplate = baton.UseTemplate;
            this.Description = baton.Description;
            this.Date = baton.Date;

            this.Templates = new List<SelectListItem>();
            foreach (SelectionItem item in baton.Templates) {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Text = item.Text;
                selectItem.Value = item.Value;
                if (Convert.ToInt32(item.Value) == this.TemplateId) {
                    selectItem.Selected = true;
                }
                this.Templates.Add(selectItem);
            }
        }
    }

    public class BatonCalendar {
        public MonthSelectorView.MonthsWithMeetings MonthsForMeetings { get; set; }
        public MonthSelectorView.MeetingsForMonth MeetingsForMonth { get; set; }
    }
}