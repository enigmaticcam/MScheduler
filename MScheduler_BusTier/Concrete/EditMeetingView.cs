using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public interface IEditMeetingView {
        int Id { get; }
        string Description { get; }
        DateTime Date { get; }
        bool HasChanged { get; }
        EditMeetingView.Baton BatonMeeting { get; set; }
        EditMeetingView.CreateMeeting BatonCreateMeeting { set; }
        Dictionary<int, string> Meetings { get; set; }
        void SetMeeting(int id);
        void CreateSlot();
        void LoadFromSource();
        void Save();
    }
    
    public class EditMeetingView : IEditMeetingView {
        private Meeting.MeetingData _data;
        private IFactory _factory;
        private ISlotFiller _slotFiller;

        public int Id {
            get { return _data.Id; }
        }

        public string Description {
            get { return _data.Description; }
        }

        public DateTime Date {
            get { return _data.Date; }
        }

        private bool _hasChanged;
        public bool HasChanged {
            get { return _hasChanged; }
        }

        public Baton BatonMeeting {
            get {
                Baton baton = new Baton();
                baton.Id = _data.Id;
                baton.Description = _data.Description;
                baton.Date = _data.Date;
                baton.IsDeleted = _data.IsDeleted;                
                List<EditMeetingView.BatonSlot> slots = new List<BatonSlot>();
                foreach (ISlot slot in _data.Slots.OrderBy(s => s.SortNumber)) {
                    BatonSlot batonSlot = new BatonSlot();
                    batonSlot.SlotId = slot.Id;
                    batonSlot.Title = slot.Title;
                    batonSlot.Description = slot.Description;
                    batonSlot.SortNumber = slot.SortNumber;
                    batonSlot.SlotTypeId = (int)slot.SlotType;
                    batonSlot.SlotTypes = SelectionItem.ConvertEnumToSelectionItem<Slot.enumSlotType>(slot.SlotType);
                    batonSlot.SlotFillerId = slot.SlotFillerId;
                    batonSlot.SlotFillers = new List<SelectionItem>();
                    batonSlot.IsDisabled = false;
                    if (slot.Id > 0) {
                        batonSlot.IsSlotTypeDisabled = true;
                    }
                    batonSlot.SlotFillers.Add(new SelectionItem("Empty", "0", false, (slot.SlotFillerId == 0)));
                    foreach (KeyValuePair<int, string> slotFiller in _slotFiller.SlotFillersForType(slot.SlotType)) {
                        batonSlot.SlotFillers.Add(new SelectionItem(slotFiller.Value, slotFiller.Key.ToString(), false, (slot.SlotFillerId == slotFiller.Key)));
                    }
                    slots.Add(batonSlot);
                }
                baton.BatonSlots = slots;
                return baton;
            }
            set {
                _data.Description = value.Description;
                _data.IsDeleted = value.IsDeleted;
                foreach (EditMeetingView.BatonSlot batonSlot in value.BatonSlots) {
                    if (!batonSlot.IsDisabled) {
                        ISlot slot = _data.Slots.Where(s => s.Id == batonSlot.SlotId).First();
                        slot.Description = batonSlot.Description;
                        slot.Title = batonSlot.Title;
                        slot.SortNumber = batonSlot.SortNumber;
                        slot.SlotType = (Slot.enumSlotType)batonSlot.SlotTypeId;
                        slot.FillSlot(batonSlot.SlotFillerId);
                    }
                }
                _hasChanged = true;
            }
        }

        public CreateMeeting BatonCreateMeeting {
            set {
                _data = new Meeting.MeetingData();
                if (value.UseTemplate) {
                    ITemplate template = _factory.CreateTemplate();
                    template.LoadFromSource(value.TemplateId);
                    _data.Slots = template.GenerateMeetingSlots();
                }
                _data.Date = value.Date;
                _data.Description = value.Description;
                IMeeting meeting = _factory.CreateMeeting();
                meeting.Data = _data;
                int meetingId = meeting.SaveToSource();
                meeting = _factory.CreateMeeting();
                meeting.LoadFromSource(meetingId);
                _data = meeting.Data;
            }
        }

        private Dictionary<int, string> _meetings;
        public Dictionary<int, string> Meetings {
            get { return _meetings; }
            set { _meetings = value; }
        }

        public void SetMeeting(int id) {
            if (_data == null || _data.Id != id) {
                IMeeting meeting = _factory.CreateMeeting();
                meeting.LoadFromSource(id);
                _data = meeting.Data;
                _hasChanged = false;
            }
        }

        public void CreateSlot() {
            ISlot slot = _factory.CreateSlot();
            _data.Slots.Add(slot);            
        }

        public void LoadFromSource() {

        }

        public void Save() {
            IMeeting meeting = _factory.CreateMeeting();
            meeting.Data = _data;
            int meetingId = meeting.SaveToSource();
            meeting.LoadFromSource(meetingId);
            _data = meeting.Data;
            _hasChanged = false;
        }

        public EditMeetingView(IFactory factory, ISlotFiller slotFiller) {
            _factory = factory;
            _slotFiller = slotFiller;
        }

        public class CreateMeeting {
            public List<SelectionItem> Templates { get; set; }
            public int TemplateId { get; set; }
            public bool UseTemplate { get; set; }
            public string Description { get; set; }
            public DateTime Date { get; set; }
        }

        public class Baton {
            public int Id { get; set; }
            public string Description { get; set; }
            public DateTime Date { get; set; }
            public bool IsDeleted { get; set; }
            public List<EditMeetingView.BatonSlot> BatonSlots { get; set; }
        }

        public class BatonSlot {
            public int SlotId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int SlotFillerId { get; set; }
            public int SortNumber { get; set; }
            public int SlotTypeId { get; set; }
            public bool IsDisabled { get; set; }
            public List<SelectionItem> SlotFillers { get; set; }
            public List<SelectionItem> SlotTypes { get; set; }

            private bool _isSlotTypeDisabled;
            public bool IsSlotTypeDisabled {
                get {
                    if (this.IsDisabled) {
                        return true;
                    } else {
                        return _isSlotTypeDisabled;
                    }
                }
                set { _isSlotTypeDisabled = value; }
            }
        }
    }

    public abstract class EditMeetingViewDecorator : IEditMeetingView {
        private IEditMeetingView _meeting;

        public virtual int Id {
            get { return _meeting.Id; }
        }

        public virtual string Description {
            get { return _meeting.Description; }
        }

        public virtual DateTime Date {
            get { return _meeting.Date; }
        }

        public virtual bool HasChanged {
            get { return _meeting.HasChanged; }
        }

        public virtual Dictionary<int, string> Meetings {
            get { return _meeting.Meetings; }
            set { _meeting.Meetings = value; }
        }

        public virtual EditMeetingView.CreateMeeting BatonCreateMeeting {
            set { _meeting.BatonCreateMeeting = value; }
        }

        public virtual EditMeetingView.Baton BatonMeeting {
            get { return _meeting.BatonMeeting; }
            set { _meeting.BatonMeeting = value; }
        }

        public virtual void SetMeeting(int id) {
            _meeting.SetMeeting(id);
        }

        public virtual void LoadFromSource() {
            _meeting.LoadFromSource();
        }

        public virtual void Save() {
            _meeting.Save();
        }

        public virtual void CreateSlot() {
            _meeting.CreateSlot();
        }

        public EditMeetingViewDecorator(IEditMeetingView meeting) {
            _meeting = meeting;
        }
    }
}
