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
        IEnumerable<EditMeetingView.BatonSlot> BatonSlots { get; set; }
        Dictionary<int, string> Meetings { get; set; }
        void SetMeeting(int id);
        void LoadFromSource();
    }
    
    public class EditMeetingView : IEditMeetingView {
        private Meeting.MeetingData _data;
        private IFactory _factory;

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
                return baton;
            }
            set {

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

        public IEnumerable<EditMeetingView.BatonSlot> BatonSlots {
            get {
                foreach (ISlot slot in _data.Slots.OrderBy(s => s.SortNumber)) {
                    BatonSlot baton = new BatonSlot();
                    baton.SlotId = slot.Id;
                    baton.Title = slot.Title;
                    baton.Description = slot.Description;
                    baton.SortNumber = slot.SortNumber;
                    baton.SlotType = slot.SlotType.ToString();
                    yield return baton;
                }
            }
            set {

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

        public EditMeetingView(IFactory factory) {
            _factory = factory;
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
        }

        public class BatonSlot {
            public int SlotId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string SlotFillerId { get; set; }
            public string FillerDescription { get; set; }
            public int SortNumber { get; set; }
            public string SlotType { get; set; }
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

        public virtual IEnumerable<EditMeetingView.BatonSlot> BatonSlots {
            get { return _meeting.BatonSlots; }
            set { _meeting.BatonSlots = value; }
        }

        public virtual void SetMeeting(int id) {
            _meeting.SetMeeting(id);
        }

        public virtual void LoadFromSource() {
            _meeting.LoadFromSource();
        }

        public EditMeetingViewDecorator(IEditMeetingView meeting) {
            _meeting = meeting;
        }
    }
}
