using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface IMeeting {
        public int Id;
        public string Description;
        public DateTime Date;
        public IEnumerable<ISlot> Slots;
    }

    public abstract class Meeting {
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

        public class MeetingData {
            public int Id;
            public string Description;
            public DateTime Date;
            public List<ISlot> Slots { get; set; }
        }
    }

    public abstract class MeetingDecorator : IMeeting {
        private IMeeting _meeting;
        public MeetingDecorator(IMeeting meeting) {
            _meeting = meeting;
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
    }
}
