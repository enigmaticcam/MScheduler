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
        private IFactory _factory;
        private DataSet _dataSet;

        public override void LoadFromSource(int id) {
            ActionLoadMeeting action = new ActionLoadMeeting.Builder()
                .SetConnection(_connection)
                .SetFactory(_factory)
                .SetMeetingId(id)
                .Build();
            _meeting.Data = action.PerformAction();
        }

        public override int SaveToSource() {
            ActionSaveMeeting action = new ActionSaveMeeting(_meeting, _connection);
            return action.PerformAction();
        }

        public MeetingDecoratorDatabase(Builder builder) : base(builder.Meeting) {
            _meeting = builder.Meeting;
            _connection = builder.Connection;
            _factory = builder.Factory;
        }

        public class ActionLoadMeeting {
            private IConnectionControl _connection;
            private IFactory _factory;
            private DataSet _dataSet;
            private int _meetingId;
            private Meeting.MeetingData _data = new Meeting.MeetingData();
            private StringBuilder _sql = new StringBuilder();

            public Meeting.MeetingData PerformAction() {
                GenerateLoadSql();
                GetDataSetFromDatabase();
                PopulateDataFromDataSet();
                PopulateSlots();
                return _data;
            }

            private void GenerateLoadSql() {
                _sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Meeting");
                _sql.AppendLine("where MeetingId = " + _meetingId);
                _sql.AppendLine("");
                _sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Slot");
                _sql.AppendLine("where MeetingId = " + _meetingId);
            }

            private void GetDataSetFromDatabase() {
                _dataSet = _connection.ExecuteDataSet(_sql.ToString());
            }

            private void PopulateDataFromDataSet() {
                _data.Id = _meetingId;
                _data.Description = GetValueFromDataSet("MeetingDescription").ToString();
                _data.Date = (DateTime)GetValueFromDataSet("MeetingDate");
            }

            private void PopulateSlots() {
                List<ISlot> slots = new List<ISlot>();
                foreach (DataRow dr in _dataSet.Tables[1].Rows) {
                    ISlot slot = _factory.CreateSlot();
                    slot.LoadFromSource((int)dr["SlotId"]);
                    slots.Add(slot);
                }
                _data.Slots = slots;
            }

            private object GetValueFromDataSet(string columnName) {
                return _dataSet.Tables[0].Rows[0][columnName];
            }

            public ActionLoadMeeting(Builder builder) {
                _factory = builder.Factory;
                _connection = builder.Connection;
                _meetingId = builder.MeetingId;
            }

            public class Builder {
                public IFactory Factory;
                public IConnectionControl Connection;
                public int MeetingId;

                public Builder SetFactory(IFactory factory) {
                    this.Factory = factory;
                    return this;
                }

                public Builder SetConnection(IConnectionControl connection) {
                    this.Connection = connection;
                    return this;
                }

                public Builder SetMeetingId(int meetingId) {
                    this.MeetingId = meetingId;
                    return this;
                }

                public ActionLoadMeeting Build() {
                    return new ActionLoadMeeting(this);
                }
            }
        }

        public class ActionSaveMeeting {
            private IConnectionControl _connection;
            private IMeeting _meeting;
            private StringBuilder _sql = new StringBuilder();

            public int PerformAction() {
                PrepareSql();
                GenerateTableSql();
                AddNewIdReturn();
                CloseSQL();
                int id = PerformSave();
                SaveSlots();
                return id;
            }

            private void PrepareSql() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionOpen());
                _sql.AppendLine("declare @MeetingId int");
            }

            private void GenerateTableSql() {
                if (_meeting.Id <= 0) {
                    GenerateTableSqlNew();
                } else {
                    GenerateTableSqlExisting();
                }
            }

            private void GenerateTableSqlNew() {
                _sql.AppendLine("insert into " + _connection.DatabaseName + ".dbo.Meeting(MeetingDescription, MeetingDate)");
                _sql.Append("values('" + _connection.SqlSafe(_meeting.Description) + "'");
                _sql.Append(",'" + _meeting.Date.ToShortDateString() + "'");
                _sql.AppendLine(")");
                _sql.AppendLine("");
                _sql.AppendLine("select @MeetingId = max(MeetingID) from " + _connection.DatabaseName + ".dbo.Meeting");
            }

            private void GenerateTableSqlExisting() {
                _sql.AppendLine("update " + _connection.DatabaseName + ".dbo.Meeting");
                _sql.AppendLine("set MeetingDescription = '" + _connection.SqlSafe(_meeting.Description) + "'");
                _sql.AppendLine(", MeetingDate = '" + _meeting.Date.ToShortDateString() + "'");
                _sql.AppendLine("where MeetingId = " + _meeting.Id);
                _sql.AppendLine("");
                _sql.AppendLine("set @MeetingId = " + _meeting.Id);
            }

            private void AddNewIdReturn() {
                _sql.AppendLine("");
                _sql.AppendLine("select @MeetingId");
            }

            private void CloseSQL() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionClose());
            }

            private int PerformSave() {
                return (int)_connection.ExecuteScalar(_sql.ToString());
            }

            private void SaveSlots() {
                foreach (ISlot slot in _meeting.Slots) {
                    slot.SaveToSource();
                }
            }

            public ActionSaveMeeting(IMeeting meeting, IConnectionControl connection) {
                _meeting = meeting;
                _connection = connection;
            }
        }

        public class Builder {
            public IMeeting Meeting;
            public IConnectionControl Connection;
            public IFactory Factory;

            public Builder SetMeeting(IMeeting meeting) {
                this.Meeting = meeting;
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

            public MeetingDecoratorDatabase Build() {
                return new MeetingDecoratorDatabase(this);
            }
        }
    }

    public class SlotDecoratorDatabase : SlotDecorator {
        private ISlot _slot;
        private IConnectionControl _connection;
        private IFactory _factory;

        public override void LoadFromSource(int id) {
            ActionLoadSlot action = new ActionLoadSlot.Builder()
                .SetConnection(_connection)
                .SetFactory(_factory)
                .SetSlotId(id)
                .Build();
            _slot.Data = action.PerformAction();
        }

        public override int SaveToSource() {
            ActionSaveSlot action = new ActionSaveSlot(_connection, _slot);
            return action.PerformAction();
        }

        public class ActionLoadSlot {
            private IConnectionControl _connection;
            private IFactory _factory;
            private DataSet _dataSet;
            private int _slotId;
            private Slot.SlotData _data = new Slot.SlotData();
            private StringBuilder _sql = new StringBuilder();

            public Slot.SlotData PerformAction() {
                GenerateLoadSql();
                GetDataSetFromDatabase();
                PopulateDataFromDataSet();
                return _data;
            }

            private void GenerateLoadSql() {
                _sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Slot");
                _sql.AppendLine("where SlotId = " + _slotId);
            }

            private void GetDataSetFromDatabase() {
                _dataSet = _connection.ExecuteDataSet(_sql.ToString());
            }

            private void PopulateDataFromDataSet() {
                _data.Id = _slotId;
                _data.IsDeleted = false;
                _data.MeetingId = (int)GetValueFromDataSet("MeetingId");
                _data.SortNumber = (int)GetValueFromDataSet("SortNumber");
                _data.Title = GetValueFromDataSet("Title").ToString();
                _data.Description = GetValueFromDataSet("Description").ToString();
                _data.Filler = _factory.CreateSlotFiller((int)GetValueFromDataSet("SlotFillerId"));
            }

            private object GetValueFromDataSet(string columnName) {
                return _dataSet.Tables[0].Rows[0][columnName];
            }

            public ActionLoadSlot(Builder builder) {
                _factory = builder.Factory;
                _connection = builder.Connection;
                _slotId = builder.SlotId;
            }

            public class Builder {
                public IFactory Factory;
                public IConnectionControl Connection;
                public int SlotId;

                public Builder SetFactory(IFactory factory) {
                    this.Factory = factory;
                    return this;
                }

                public Builder SetConnection(IConnectionControl connection) {
                    this.Connection = connection;
                    return this;
                }

                public Builder SetSlotId(int slotId) {
                    this.SlotId = slotId;
                    return this;
                }

                public ActionLoadSlot Build() {
                    return new ActionLoadSlot(this);
                }
            }
        }

        public class ActionSaveSlot {
            private IConnectionControl _connection;
            private ISlot _slot;
            private StringBuilder _sql = new StringBuilder();

            public int PerformAction() {
                PrepareSql();
                GenerateTableSql();
                AddNewIdReturn();
                CloseSQL();
                int id = PerformSave();
                return id;
            }

            private void PrepareSql() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionOpen());
                _sql.AppendLine("declare @SlotId int");
            }

            private void GenerateTableSql() {
                if (_slot.Id <= 0) {
                    GenerateTableSqlNew();
                } else {
                    GenerateTableSqlExisting();
                }
            }

            private void GenerateTableSqlNew() {
                _sql.AppendLine("insert into " + _connection.DatabaseName + ".dbo.Slot(MeetingId, SlotFillerId, Title, Description, SortNumber)");
                _sql.Append("values(" + _slot.MeetingId);
                _sql.Append("," + _slot.SlotFillerId);
                _sql.Append(",'" + _connection.SqlSafe(_slot.Title) + "'");
                _sql.Append(",'" + _connection.SqlSafe(_slot.Description) + "'");
                _sql.Append("," + _slot.SortNumber);
                _sql.AppendLine(")");
                _sql.AppendLine("");
                _sql.AppendLine("select @SlotId = max(SlotId) from " + _connection.DatabaseName + ".dbo.Slot");
            }

            private void GenerateTableSqlExisting() {
                _sql.AppendLine("update " + _connection.DatabaseName + ".dbo.Slot");
                _sql.AppendLine("set MeetingId = " + _slot.MeetingId);
                _sql.AppendLine(", SlotFillerId = " + _slot.SlotFillerId);
                _sql.AppendLine(", Title = '" + _connection.SqlSafe(_slot.Title) + "'");
                _sql.AppendLine(", Description = '" + _connection.SqlSafe(_slot.Description) + "'");
                _sql.AppendLine(", SortNumber = " + _slot.SortNumber);
                _sql.AppendLine("where SlotId = " + _slot.Id);
                _sql.AppendLine("");
                _sql.AppendLine("set @SlotId = " + _slot.Id);
            }

            private void AddNewIdReturn() {
                _sql.AppendLine("");
                _sql.AppendLine("select @SlotId");
            }

            private void CloseSQL() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionClose());
            }

            private int PerformSave() {
                return (int)_connection.ExecuteScalar(_sql.ToString());
            }

            public ActionSaveSlot(IConnectionControl connection, ISlot slot) {
                _connection = connection;
                _slot = slot;
            }
        }

        public SlotDecoratorDatabase(Builder builder): base(builder.Slot) {
            _connection = builder.Connection;
            _factory = builder.Factory;
            _slot = builder.Slot;
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
