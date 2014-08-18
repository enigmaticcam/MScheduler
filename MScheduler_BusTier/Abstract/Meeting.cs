using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface IMeeting {
        Meeting.MeetingData Data { set; }
        int Id { get; }
        string Description { get; }
        DateTime Date { get; }
        IEnumerable<ISlot> Slots { get; }
        void LoadFromSource(int id);
        int SaveToSource();
    }

    public abstract class Meeting : IMeeting {
        private MeetingData _meetingData;
        protected MeetingData PrivateData {
            get { return _meetingData; }
            set { _meetingData = value; }
        }
        public MeetingData Data {
            set { _meetingData = value; }
        }

        public int Id {
            get { return _meetingData.Id; }
        }

        public string Description {
            get { return _meetingData.Description; }
        }

        public DateTime Date {
            get { return _meetingData.Date; }
        }

        public IEnumerable<ISlot> Slots {
            get { return _meetingData.Slots.OrderBy(s => s.SortNumber); }
        }

        public void LoadFromSource(int id) {
            // To be implemented by decorators
        }

        public int SaveToSource() {
            // To be implemented by decorators
            return 0;
        }

        public class MeetingData {
            public int Id;
            public string Description;
            public DateTime Date;
            public List<ISlot> Slots { get; set; }
        }
    }

    public abstract class MeetingDecorator : IMeeting {
        private IMeeting _meeting;

        public virtual Meeting.MeetingData Data {
            set { _meeting.Data = value; }
        }

        public virtual int Id {
            get { return _meeting.Id; }
        }

        public virtual string Description {
            get { return _meeting.Description; }
        }

        public virtual DateTime Date {
            get { return _meeting.Date; }
        }

        public virtual IEnumerable<ISlot> Slots {
            get { return _meeting.Slots; }
        }

        public virtual void LoadFromSource(int id) {
            _meeting.LoadFromSource(id);
        }

        public virtual int SaveToSource() {
            return _meeting.SaveToSource();
        }

        public MeetingDecorator(IMeeting meeting) {
            _meeting = meeting;
        }
    }
}
