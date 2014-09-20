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

        public IEditMeetingView CurrentEditMeetingView {
            get { return _data.EditMeetingView; }
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

        public MvcHtmlString DisplayTemplateSlotSelector() {
            ViewControlDropDownList control = new ViewControlDropDownList();
            control.Text = "Template Slots";
            control.HtmlId = "TemplateSlotSelectorId";
            control.TextIfThereAreNoItems = "No Slots have yet been created";
            control.Items = ConvertListToSelectListItem<TemplateSlot>(_data.EditTemplateView.Slots, "Id", "Title", _data.EditTemplateView.CurrentSlotId);
            return _data.ViewBuilder.DisplayViewControlDropDownList(control);
        }

        public MvcHtmlString DisplayTemplateSlotTable() {
            List<BatonTemplateSlots> batons = new List<BatonTemplateSlots>();
            List<BatonTemplateSlot> slots = new List<BatonTemplateSlot>();
            foreach (TemplateSlot slot in _data.EditTemplateView.Slots) {
                BatonTemplateSlot batonSlot = new BatonTemplateSlot();
                batonSlot.Import(slot);
                slots.Add(batonSlot);
            }            
            BatonTemplateSlots baton = new BatonTemplateSlots();
            baton.TemplateId = _data.EditTemplateView.Id;
            baton.TemplateSlots = slots;
            return _data.ViewBuilder.DisplayTemplateSlotTable(baton);
        }

        public MvcHtmlString DisplayMeetingCalendar() {
            BatonCalendar baton = new BatonCalendar();
            baton.MonthsForMeetings = _data.EditMeetingView.BatonMonths;
            baton.MeetingsForMonth = _data.EditMeetingView.BatonMeetings;
            return _data.ViewBuilder.DisplayMeetingCalendar(baton);
        }

        public MvcHtmlString DisplayCreateMeeting() {
            BatonCreateMeeting baton = new BatonCreateMeeting();
            baton.Import(_data.EditMeetingView.BatonCreateMeeting);
            baton.UseTemplate = true;
            return _data.ViewBuilder.DisplayCreateMeeting(baton);
        }

        private List<SelectListItem> ConvertEnumToSelectList<TEnum>(TEnum selectedValue) {
            List<SelectListItem> items = new List<SelectListItem>();
            string[] keys = Enum.GetNames(typeof(TEnum));
            Array values = Enum.GetValues(typeof(TEnum));
            for (int i = 0; i <= keys.GetUpperBound(0); i++) {
                SelectListItem item = new SelectListItem();
                item.Text = keys[i];
                item.Value = ((int)values.GetValue(i)).ToString();
                if (item.Value == selectedValue.ToString()) {
                    item.Selected = true;
                }
                items.Add(item);
            }
            return items;
        }

        private List<SelectListItem> ConvertListToSelectListItem<T>(List<T> list, string keyPropertyName, string textPropertyName, int selected) {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (T item in list) {
                SelectListItem selectItem = new SelectListItem();
                selectItem.Text = item.GetType().GetProperty(textPropertyName).GetValue(item, null).ToString();
                selectItem.Value = item.GetType().GetProperty(keyPropertyName).GetValue(item, null).ToString();
                selectItem.Selected = (selected.ToString() == selectItem.Value);
                items.Add(selectItem);
            }
            return items;
        }

        private List<SelectListItem> ConvertDictionaryToSelectListItem(Dictionary<int, string> list, int selected) {
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (int key in list.Keys) {
                SelectListItem item = new SelectListItem();
                item.Text = list[key];
                item.Value = key.ToString();
                item.Selected = (selected == key);
                items.Add(item);
            }
            return items;
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

            private IEditMeetingView _editMeetingView;
            public IEditMeetingView EditMeetingView {
                get {
                    if (_editMeetingView == null) {
                        _editMeetingView = this.Factory.CreateEditMeetingView();
                        _editMeetingView.LoadFromSource();
                    }
                    return _editMeetingView;
                }
            }
        }
    }

    
}