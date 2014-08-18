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
            Slot.SlotData data = new Slot.SlotData();
            data.MeetingId = (int)GetValueFromDataSet("MeetingId");
            data.Title = GetValueFromDataSet("Title").ToString();
            data.Description = GetValueFromDataSet("Description").ToString();
            data.SortNumber = (int)GetValueFromDataSet("SortNumber");
            data.Filler = _factory.CreateSlotFiller((int)GetValueFromDataSet("SlotFillerId"));
            _slot.Data = data;
        }

        private string SaveAsNewOrExisting() {
            if (_slot.Id <= 0) {
                return SaveAsNew();
            } else {
                return SaveAsExisting();
            }
        }

        private string SaveAsNew() {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Slot_New");
            return sql.ToString();
        }

        private string SaveAsExisting() {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Slot_Edit");
            sql.AppendLine("@SlotId = " + _slot.Id);
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

    public class TemplateDecoratorDatabase : TemplateDecorator {
        private ITemplate _template;
        private IConnectionControl _connection;
        private DataSet _dataSet;

        public override void LoadFromSource(int id) {
            string sql = "select * from " + _connection.DatabaseName + ".dbo.Template where TemplateId = " + id.ToString();
            sql += " select * from " + _connection.DatabaseName + ".dbo.TemplateSlot where TemplateId = " + id.ToString();
            _dataSet = _connection.ExecuteDataSet(sql);
            PopulateDataFromDataSet();
        }

        public override int SaveToSource() {
            ActionSaveTemplate action = new ActionSaveTemplate(_template, _connection);
            return action.PerformAction();
        }

        private void PopulateDataFromDataSet() {
            Template.TemplateData data = new Template.TemplateData();
            data.Description = GetValueFromDataSet("Description").ToString();
            data.TemplateSlots = GetTemplateSlots(_template.Id);
            _template.Data = data;
        }

        private List<TemplateSlot> GetTemplateSlots(int templateId) {
            List<TemplateSlot> templateSlots = new List<TemplateSlot>();
            foreach (DataRow dr in _dataSet.Tables[1].Rows) {
                templateSlots.Add(GetTemplateSlotFromDataRow(dr));
            }
            return templateSlots;
        }

        private TemplateSlot GetTemplateSlotFromDataRow(DataRow dr) {
            TemplateSlot templateSlot = new TemplateSlot();
            templateSlot.Id = (int)dr["TemplateId"];
            templateSlot.SlotType = (Slot.enumSlotType)dr["SlotTypeId"];
            templateSlot.Title = dr["Title"].ToString();
            templateSlot.SortNumber = (int)dr["SortNumber"];
            return templateSlot;
        }

        private object GetValueFromDataSet(string columnName) {
            return _dataSet.Tables[0].Rows[0][columnName];
        }

        public TemplateDecoratorDatabase(ITemplate template, IConnectionControl connection) : base(template) {
            _template = template;
            _connection = connection;
        }

        public class ActionSaveTemplate {
            private StringBuilder _sql = new StringBuilder();
            private IConnectionControl _connection;
            private ITemplate _template;

            public int PerformAction() {
                GenerateTemplateSql();
                GenerateTemplateSlotSql();
                return PerformSave();
            }

            private void GenerateTemplateSql() {
                SaveAsNewOrExisting();
                _sql.AppendLine("@Description = '" + _connection.SqlSafe(_template.Description));
            }

            private void SaveAsNewOrExisting() {
                _sql.Append(_connection.GenerateSqlTryWithTransactionOpen());
                _sql.AppendLine("");
                if (_template.Id <= 0) {
                    GenerateSaveAsNewParameters();
                    GenerateSaveParameters();
                    GenerateSaveAsNewSelectId();
                } else {
                    GenerateSaveAsExistingParameters();
                    GenerateSaveParameters();
                    GenerateSaveAsExistingSelectId();
                }
                _sql.AppendLine("");
                _sql.Append(_connection.GenerateSqlTryWithTransactionClose());
            }

            private void GenerateSaveParameters() {
                _sql.AppendLine("@Description = '" + _connection.SqlSafe(_template.Description));
            }

            private void GenerateSaveAsNewParameters() {                
                _sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Template_New");
            }

            private string GenerateSaveAsExistingParameters() {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("exec " + _connection.DatabaseName + ".dbo.Template_Edit");
                sql.AppendLine("@TemplateId = " + _template.Id);
                sql.Append(",");
                return sql.ToString();
            }

            private void GenerateSaveAsNewSelectId() {
                _sql.AppendLine("");
                _sql.AppendLine("delcare @TemplateId int");
                _sql.AppendLine("select @TemplateId = max(TemplateId) from " + _connection.DatabaseName + ".dbo.Template");
            }

            private void GenerateSaveAsExistingSelectId() {
                _sql.AppendLine("");
                _sql.AppendLine("delcare @TemplateId int");
                _sql.AppendLine("select @TemplateId = " + _template.Id);
            }

            private void GenerateTemplateSlotSql() {
                _sql.AppendLine("");
                if (_template.Id > 0) {
                    RemoveExistingTemplateSlots();
                }
                foreach (TemplateSlot templateSlot in _template.TemplateSlots) {
                    GenerateIndividualTemplateSlotSql(templateSlot);
                }
            }

            private void GenerateIndividualTemplateSlotSql(TemplateSlot templateSlot) {
                _sql.AppendLine("exec TemplateSlot_New");
                _sql.AppendLine(" @TemplateId = @TemplateId");
                _sql.AppendLine(", @SlotTypeId = " + (int)templateSlot.SlotType);
                _sql.AppendLine(", @Title = '" + _connection.SqlSafe(templateSlot.Title + "'"));
                _sql.AppendLine(", @SortNumber = " + templateSlot.SortNumber);
            }

            private void RemoveExistingTemplateSlots() {
                _sql.AppendLine("delete from " + _connection.DatabaseName + ".dbo.TemplateSlot where TemplateId = " + _template.Id);
            }

            private int PerformSave() {
                return (int)_connection.ExecuteScalar(_sql.ToString());
            }

            public ActionSaveTemplate(ITemplate template, IConnectionControl connnection) {
                _template = template;
                _connection = connnection;
            }
        }
    }
}
