using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Concrete {
    public interface IMonthSelectorView {
        DateTime CurrentMonth { get; set; }
        int CurrentDay { get; set; }
        Dictionary<int, string> Templates { get; set; }
        MonthSelectorView.MonthsWithMeetings BatonMonths { get; }
        MonthSelectorView.MeetingsForMonth BatonMeetings { get; }
        EditMeetingView.CreateMeeting BatonCreateMeeting { get; }
        void LoadFromSource();
    }

    public class MonthSelectorView : IMonthSelectorView {
        private DateTime _currentMonth;
        public DateTime CurrentMonth {
            get { return _currentMonth; }
            set { _currentMonth = value; }
        }

        private int _currentDay;
        public int CurrentDay {
            get { return _currentDay; }
            set { _currentDay = value; }
        }

        private Dictionary<int, string> _templates;
        public Dictionary<int, string> Templates {
            get { return _templates; }
            set { _templates = value; }
        }

        public EditMeetingView.CreateMeeting BatonCreateMeeting {
            get {
                EditMeetingView.CreateMeeting baton = new EditMeetingView.CreateMeeting();
                baton.Templates = new List<SelectionItem>();
                foreach (KeyValuePair<int, string> template in _templates) {
                    SelectionItem item = new SelectionItem(template.Value, template.Key.ToString());
                    baton.Templates.Add(item);
                }
                baton.Date = _currentMonth.AddDays(_currentDay);
                return baton;
            }
        }

        public MonthsWithMeetings BatonMonths {
            get {
                MonthsWithMeetings month = new MonthsWithMeetings(_currentMonth);
                return month;
            }
        }

        public MeetingsForMonth BatonMeetings {
            get { return null; }
        }

        public void LoadFromSource() {

        }

        public MonthSelectorView() {
            _currentMonth = DateTime.Parse(DateTime.Now.Year + "-" + DateTime.Now.Month);
        }

        public class MonthsWithMeetings {
            private DateTime _date;
            public string MonthName { get; set; }
            public List<ExtraMonth> ExtraMonths { get; set; }
            public string[] MonthDays { get; set; }

            public MonthsWithMeetings(DateTime date) {
                _date = date;
                this.MonthName = YearMonthFromDate(_date);
                PopulateMonthDays();
                PopulateExtraMonthNames();
            }

            private string YearMonthFromDate(DateTime date) {
                return date.ToString("MMM yyyy", CultureInfo.InvariantCulture);
            }

            private void PopulateMonthDays() {
                this.MonthDays = new string[42];
                int start = 0;
                switch (_date.DayOfWeek) {
                    case DayOfWeek.Sunday:
                        start = 0;
                        break;
                    case DayOfWeek.Monday:
                        start = 1;
                        break;
                    case DayOfWeek.Tuesday:
                        start = 2;
                        break;
                    case DayOfWeek.Wednesday:
                        start = 3;
                        break;
                    case DayOfWeek.Thursday:
                        start = 4;
                        break;
                    case DayOfWeek.Friday:
                        start = 5;
                        break;
                    case DayOfWeek.Saturday:
                        start = 6;
                        break;
                }
                for (int i = 0; i < DateTime.DaysInMonth(_date.Year, _date.Month); i++) {
                    this.MonthDays[i + start] = (i + 1).ToString();
                }
            }

            private void PopulateExtraMonthNames() {
                this.ExtraMonths = new List<ExtraMonth>();
                for (int i = 0; i < 13; i++) {
                    this.ExtraMonths.Add(new ExtraMonth(YearMonthFromDate(_date.AddMonths(-6 + i)), -6 + i, (i == 6)));
                }
            }

            public class ExtraMonth {
                public string Name { get; set; }
                public int MonthOffset { get; set; }
                public bool IsCurrentMonth { get; set; }

                public ExtraMonth(string name, int monthOffset, bool isCurrentMonth) {
                    this.Name = name;
                    this.MonthOffset = monthOffset;
                    this.IsCurrentMonth = isCurrentMonth;
                }
            }
        }

        public class MeetingsForMonth {
            private Dictionary<int, List<int>> _meetingDates = new Dictionary<int, List<int>>();
            public IEnumerable<int> MeetingForDay(int day) {
                if (_meetingDates.ContainsKey(day)) {
                    return _meetingDates[day];
                } else {
                    return null;
                }
            }

            private Dictionary<int, string> _meetingNames = new Dictionary<int, string>();
            public string MeetingName(int meetingId) {
                return _meetingNames[meetingId];
            }

            public void AddMeeting(int meetingId, int dayOfMonth, string meetingName) {
                if (!_meetingDates.ContainsKey(dayOfMonth)) {
                    _meetingDates.Add(dayOfMonth, new List<int>());
                }
                _meetingDates[dayOfMonth].Add(meetingId);
                _meetingNames.Add(meetingId, meetingName);
            }
        }
    }

    public abstract class MonthSelectorViewDecorator : IMonthSelectorView {
        private IMonthSelectorView _monthSelector;

        public virtual DateTime CurrentMonth {
            get { return _monthSelector.CurrentMonth; }
            set { _monthSelector.CurrentMonth = value; }
        }

        public virtual int CurrentDay {
            get { return _monthSelector.CurrentDay; }
            set { _monthSelector.CurrentDay = value; }
        }

        public virtual Dictionary<int, string> Templates {
            get { return _monthSelector.Templates; }
            set { _monthSelector.Templates = value; }
        }

        public virtual MonthSelectorView.MonthsWithMeetings BatonMonths {
            get { return _monthSelector.BatonMonths; }
        }

        public virtual EditMeetingView.CreateMeeting BatonCreateMeeting {
            get { return _monthSelector.BatonCreateMeeting; }
        }

        public virtual MonthSelectorView.MeetingsForMonth BatonMeetings {
            get { return _monthSelector.BatonMeetings; }
        }

        public virtual void LoadFromSource() {
            _monthSelector.LoadFromSource();
        }

        public MonthSelectorViewDecorator(IMonthSelectorView monthSelector) {
            _monthSelector = monthSelector;
        }
    }
}
