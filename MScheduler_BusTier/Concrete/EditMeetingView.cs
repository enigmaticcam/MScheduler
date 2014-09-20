using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public interface IEditMeetingView {
        DateTime CurrentMonth { get; set; }
        EditMeetingView.MonthWithMeetings BatonMonth { get; }
        EditMeetingView.CreateMeeting BatonCreateMeeting { get; set; }
        Dictionary<int, string> Templates { get; set; }
        void LoadFromSource();
    }
    
    public class EditMeetingView : IEditMeetingView {
        private DateTime _currentMonth;
        public DateTime CurrentMonth {
            get { return _currentMonth; }
            set { _currentMonth = value; }
        }

        public EditMeetingView.MonthWithMeetings BatonMonth {
            get {
                MonthWithMeetings month = new MonthWithMeetings(_currentMonth);
                return month;
            }
        }

        public CreateMeeting BatonCreateMeeting {
            get {
                CreateMeeting baton = new CreateMeeting();
                baton.Templates = new List<SelectionItem>();
                foreach (KeyValuePair<int, string> template in _templates) {
                    SelectionItem item = new SelectionItem(template.Value, template.Key.ToString());
                    baton.Templates.Add(item);
                }
                return baton;
            }
            set {

            }
        }

        private Dictionary<int, string> _templates;
        public Dictionary<int, string> Templates {
            get { return _templates; }
            set { _templates = value; }
        }

        public void LoadFromSource() {

        }

        public EditMeetingView() {
            _currentMonth = DateTime.Parse(DateTime.Now.Year + "-" + DateTime.Now.Month);
        }

        public class MonthWithMeetings {
            private DateTime _date;
            public string MonthName { get; set; }
            public List<ExtraMonth> ExtraMonths { get; set; }
            public string[] MonthDays { get; set; }

            public MonthWithMeetings(DateTime date) {
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

        public class CreateMeeting {
            public List<SelectionItem> Templates { get; set; }
            public int TemplateId { get; set; }
            public bool UseTemplate { get; set; }
            public string Description { get; set; }
            public DateTime Date { get; set; }
        }
    }

    public abstract class EditMeetingViewDecorator : IEditMeetingView {
        private IEditMeetingView _meeting;

        public virtual DateTime CurrentMonth {
            get { return _meeting.CurrentMonth; }
            set { _meeting.CurrentMonth = value; }
        }

        public virtual Dictionary<int, string> Templates {
            get { return _meeting.Templates; }
            set { _meeting.Templates = value; }
        }

        public virtual EditMeetingView.MonthWithMeetings BatonMonth {
            get { return _meeting.BatonMonth; }
        }

        public virtual EditMeetingView.CreateMeeting BatonCreateMeeting {
            get { return _meeting.BatonCreateMeeting; }
            set { _meeting.BatonCreateMeeting = value; }
        }

        public virtual void LoadFromSource() {
            _meeting.LoadFromSource();
        }

        public EditMeetingViewDecorator(IEditMeetingView meeting) {
            _meeting = meeting;
        }
    }
}
