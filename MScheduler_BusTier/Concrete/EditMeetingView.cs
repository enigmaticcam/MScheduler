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
        EditMeetingView.CreateSlot BatonCreateSlot { get; set; }
        Dictionary<int, string> Meetings { get; set; }
        void SetMeeting(int id);
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
                    batonSlot.SlotType = slot.SlotType.ToString();
                    batonSlot.SlotFillerId = slot.SlotFillerId;
                    batonSlot.SlotFillers = new List<SelectionItem>();
                    batonSlot.IsDisabled = false;
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
                AutoOrdererChangeCache<ISlot> autoOrderer = new AutoOrdererChangeCache<ISlot>("SortNumber");
                _data.Description = value.Description;
                _data.IsDeleted = value.IsDeleted;
                foreach (EditMeetingView.BatonSlot batonSlot in value.BatonSlots) {
                    if (!batonSlot.IsDisabled) {
                        int slotIndex = _data.Slots.FindIndex(s => s.Id == batonSlot.SlotId);
                        ISlot slot = _data.Slots.ElementAt(slotIndex);
                        slot.Description = batonSlot.Description;
                        slot.Title = batonSlot.Title;
                        if (slot.SortNumber != batonSlot.SortNumber) {
                            autoOrderer.AddSlotChange(slotIndex, slot.SortNumber, batonSlot.SortNumber);                            
                        }
                        slot.FillSlot(batonSlot.SlotFillerId);
                    }
                }
                autoOrderer.PerformAutoSort(_data.Slots);
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

        public CreateSlot BatonCreateSlot {
            get {
                CreateSlot baton = new CreateSlot();
                baton.MeetingId = _data.Id;
                baton.SlotTypes = SelectionItem.ConvertEnumToSelectionItem<Slot.enumSlotType>(Slot.enumSlotType.None);
                AutoOrderer<ISlot> autoOrder = new AutoOrderer<ISlot>(_data.Slots, "SortNumber");
                baton.SortNumber = autoOrder.NextHighestId();
                return baton;
            }
            set {
                ISlot slot = _factory.CreateSlot();
                slot.SlotType = (Slot.enumSlotType)value.SlotTypeId;
                slot.Title = value.Title;
                slot.Description = value.Description;
                _data.Slots.Add(slot);

                AutoOrderer<ISlot> autoOrder = new AutoOrderer<ISlot>(_data.Slots, "SortNumber");
                autoOrder.ChangeIdAndReorder(_data.Slots.Count - 1, value.SortNumber);
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
            public string SlotType { get; set; }
            public bool IsDisabled { get; set; }
            public List<SelectionItem> SlotFillers { get; set; }
        }

        public class CreateSlot {
            public int MeetingId { get; set; }
            public int SortNumber { get; set; }
            public int SlotTypeId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<SelectionItem> SlotTypes { get; set; }
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

        public virtual EditMeetingView.CreateSlot BatonCreateSlot {
            get { return _meeting.BatonCreateSlot; }
            set { _meeting.BatonCreateSlot = value; }
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

        public EditMeetingViewDecorator(IEditMeetingView meeting) {
            _meeting = meeting;
        }
    }
}
