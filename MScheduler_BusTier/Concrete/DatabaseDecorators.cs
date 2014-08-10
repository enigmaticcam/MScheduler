using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Abstract {

    public class MeetingDecoratorDatabase : MeetingDecorator {
        private IMeeting _meeting;
        private IConnectionControl _connection;
        private DataSet _dataSet;

        public override void LoadFromSource(int id) {
            string sql = "select * from " + _connection.DatabaseName + ".dbo.Meeting where MeetingId = " + id.ToString();
            _dataSet = _connection.ExecuteDataSet(sql);
            PopulateDataFromDataSet();
        }

        public override int SaveToSource() {
            StringBuilder sql = new StringBuilder();
            sql.Append(SaveAsNewOrExisting());
            sql.AppendLine("@Description = '" + _connection.SqlSafe(_meeting.Description));
            sql.AppendLine(", @MeetingDate = '" + _meeting.Date.ToShortDateString() + "'");
            return (int)_connection.ExecuteScalar(sql.ToString());
        }

        private void PopulateDataFromDataSet() {
            Meeting.MeetingData data = new Meeting.MeetingData();
            data.Id = (int)GetValueFromDataSet("MeetingId");
            data.Description = GetValueFromDataSet("MeetingDescription").ToString();
            data.Date = (DateTime)GetValueFromDataSet("MeetingDate");
            _meeting.Data = data;
        }

        private string SaveAsNewOrExisting() {
            if (_meeting.Id <= 0) {
                return SaveAsNew();
            } else {
                return SaveAsExisting();
            }
        }

        private string SaveAsNew() {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Meeting_New");
            return sql.ToString();
        }

        private string SaveAsExisting() {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Meeting_Edit");
            sql.AppendLine("@MeetingId = " + _meeting.Id);
            sql.Append(",");
            return sql.ToString();
        }

        private object GetValueFromDataSet(string columnName) {
            return _dataSet.Tables[0].Rows[0][columnName];
        }

        public MeetingDecoratorDatabase(IMeeting meeting, IConnectionControl connection) : base(meeting) {
            _meeting = meeting;
            _connection = connection;
        }
    }

    public class SlotDecoratorDatabase : SlotDecorator {
        private ISlot _slot;
        private IConnectionControl _connection;
        private IFactory _factory;
        private DataSet _dataSet;

        public override void LoadFromSource(int id) {
            string sql = "select * from " + _connection.DatabaseName + ".dbo.Slot where SlotId = " + id.ToString();
            _dataSet = _connection.ExecuteDataSet(sql);
            PopulateDataFromDataSet();
        }

        public override int SaveToSource() {
            StringBuilder sql = new StringBuilder();
            sql.Append(SaveAsNewOrExisting());
            sql.AppendLine("@MeetingId = " + _slot.MeetingId);
            sql.AppendLine(", @SlotFillerId = " + _slot.SlotFillerId);
            sql.AppendLine(", @Title = '" + _connection.SqlSafe(_slot.Title) + "'");
            sql.AppendLine(", @Description = '" + _connection.SqlSafe(_slot.Description) + "'");
            sql.AppendLine(", @SortNumber = " + _slot.SortNumber);
            return (int)_connection.ExecuteScalar(sql.ToString());
        }

        private void PopulateDataFromDataSet() {
            //Meeting.MeetingData data = new Meeting.MeetingData();
            //data.Id = (int)GetValueFromDataSet("MeetingId");
            //data.Description = GetValueFromDataSet("MeetingDescription").ToString();
            //data.Date = (DateTime)GetValueFromDataSet("MeetingDate");
            //_meeting.Data = data;
            Slot.SlotData data = new Slot.SlotData();

        }

        private string SaveAsNewOrExisting() {
            if (_meeting.Id <= 0) {
                return SaveAsNew();
            } else {
                return SaveAsExisting();
            }
        }

        private string SaveAsNew() {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Meeting_New");
            return sql.ToString();
        }

        private string SaveAsExisting() {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Meeting_Edit");
            sql.AppendLine("@MeetingId = " + _meeting.Id);
            sql.Append(",");
            return sql.ToString();
        }

        private object GetValueFromDataSet(string columnName) {
            return _dataSet.Tables[0].Rows[0][columnName];
        }

        public SlotDecoratorDatabase(Builder builder) : base(builder.Slot) {
            _slot = builder.Slot;
            _connection = builder.Connection;
            _factory = builder.Factory;
        }

        public class Builder {
            public ISlot Slot;
            public IConnectionControl Connection;
            public IFactory Factory;

            public Builder SetSlot(ISlot slot) {
                this.Slot = slot;
                return this;
            }

            public Builder SetConnection(IConnectionControl connection) {
                this.Connection = connection;
                return this;
            }

            public Builder SetFactory(IFactory factory) {
                this.Factory = factory;
                return this;
            }

            public SlotDecoratorDatabase Build() {
                return new SlotDecoratorDatabase(this);
            }
        }
    }
}
