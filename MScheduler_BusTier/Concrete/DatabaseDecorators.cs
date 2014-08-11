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
            templateSlot.SlotType = (Slot.SlotType)dr["SlotTypeId"];
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
