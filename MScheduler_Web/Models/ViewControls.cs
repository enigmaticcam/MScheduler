using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                 select (TemplateSlot)n.Export()).ToList();
        }
    }

    public class BatonTemplateSlot : TemplateSlot {
        public int SlotTypeId {
            get { return (int)this.SlotType; }
            set { this.SlotType = (Slot.enumSlotType)value; }
        }

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

        public void Import(TemplateSlot slot) {
            PopulateBasicProperties(slot);
        }

        public BatonTemplateSlot Export() {
            return this;
        }

        private void PopulateBasicProperties(TemplateSlot slot) {
            BatonPopulator populator = new BatonPopulator();
            populator.Populate(slot, this);
        }
    }

    public class BatonMeeting : EditMeetingView.Baton {
        public List<BatonSlot> BatonSlotsList { get; set; }

        public void Import(EditMeetingView.Baton baton) {
            PopulateBasicProperties(baton);
            PopulateBatonSlots(baton);
        }

        public BatonMeeting Export() {
            ExportBatonSlots();
            return this;
        }

        private void PopulateBasicProperties(EditMeetingView.Baton baton) {
            BatonPopulator populator = new BatonPopulator();
            populator.Populate(baton, this);
        }

        private void PopulateBatonSlots(EditMeetingView.Baton baton) {
            this.BatonSlotsList = new List<BatonSlot>();
            foreach (EditMeetingView.BatonSlot batonSlot in baton.BatonSlots) {
                BatonSlot newSlot = new BatonSlot();
                newSlot.Import(batonSlot);
                this.BatonSlotsList.Add(newSlot);
            }
        }

        private void ExportBatonSlots() {
            this.BatonSlots = new List<EditMeetingView.BatonSlot>();
            foreach (BatonSlot slot in this.BatonSlotsList) {
                this.BatonSlots.Add((EditMeetingView.BatonSlot)slot);
            }
        }
    }

    public class BatonSlot : EditMeetingView.BatonSlot {
        public List<SelectListItem> SlotFillersList { get; set; }

        public void Import(EditMeetingView.BatonSlot baton) {
            PopulateBasicProperties(baton);
            PopulateSlotFillers(baton);
        }

        private void PopulateBasicProperties(EditMeetingView.BatonSlot baton) {
            BatonPopulator populator = new BatonPopulator();
            populator.Populate(baton, this);
        }

        private void PopulateSlotFillers(EditMeetingView.BatonSlot baton) {
            this.SlotFillersList = new List<SelectListItem>();
            foreach (SelectionItem item in baton.SlotFillers) {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Text = item.Text;
                selectItem.Value = item.Value;
                selectItem.Selected = item.IsSelected;
                this.SlotFillersList.Add(selectItem);
            }
        }
    }

    public class BatonPopulator {
        public void Populate(object source, object target) {
            foreach (PropertyInfo property in source.GetType().GetProperties()) {
                if (property.CanWrite) {
                    target.GetType().GetProperty(property.Name).SetValue(target, property.GetValue(source));
                }
            }
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

    public class BatonCreateMeetingSlot {
        public int MeetingId { get; set; }
        public int SlotTypeId { get; set; }
        public int SortNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<SelectListItem> SlotTypes { get; set; }

        public void Import(EditMeetingView.CreateSlot baton) {
            this.MeetingId = baton.MeetingId;
            this.SlotTypeId = baton.SlotTypeId;
            this.SortNumber = baton.SortNumber;
            this.Title = baton.Title;
            this.Description = baton.Description;
            this.SlotTypes = new List<SelectListItem>();            
            foreach (SelectionItem item in baton.SlotTypes) {
                SelectListItem select = new SelectListItem();
                select.Text = item.Text;
                select.Value = item.Value;
                select.Selected = item.IsSelected;
                this.SlotTypes.Add(select);
            }
        }

        public EditMeetingView.CreateSlot Export() {
            EditMeetingView.CreateSlot baton = new EditMeetingView.CreateSlot();
            baton.MeetingId = this.MeetingId;
            baton.SlotTypeId = this.SlotTypeId;
            baton.SortNumber = this.SortNumber;
            baton.Title = this.Title;
            baton.Description = this.Description;
            return baton;
        }
    }

    public class BatonCalendar {
        public MonthSelectorView.MonthsWithMeetings MonthsForMeetings { get; set; }
        public MonthSelectorView.MeetingsForMonth MeetingsForMonth { get; set; }
    }
}