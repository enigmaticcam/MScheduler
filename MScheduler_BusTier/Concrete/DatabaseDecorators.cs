using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;

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
                _data.IsDeleted = (bool)GetValueFromDataSet("IsDeleted");
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
                SaveSlots(id);
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
                _sql.AppendLine("insert into " + _connection.DatabaseName + ".dbo.Meeting(MeetingDescription, MeetingDate, IsDeleted)");
                _sql.Append("values('" + _connection.SqlSafe(_meeting.Description) + "'");
                _sql.Append(",'" + _meeting.Date.ToShortDateString() + "'");
                _sql.Append("," + (_meeting.IsDeleted ? "1" : "0"));
                _sql.AppendLine(")");
                _sql.AppendLine("");
                _sql.AppendLine("select @MeetingId = max(MeetingID) from " + _connection.DatabaseName + ".dbo.Meeting");
            }

            private void GenerateTableSqlExisting() {
                _sql.AppendLine("update " + _connection.DatabaseName + ".dbo.Meeting");
                _sql.AppendLine("set MeetingDescription = '" + _connection.SqlSafe(_meeting.Description) + "'");
                _sql.AppendLine(", MeetingDate = '" + _meeting.Date.ToShortDateString() + "'");
                _sql.AppendLine(", IsDeleted = " + (_meeting.IsDeleted ? "1" : "0"));
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

            private void SaveSlots(int id) {
                foreach (ISlot slot in _meeting.Slots) {
                    slot.MeetingId = id;
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

    public class TemplateDecoratorDatabase : TemplateDecorator {
        private ITemplate _template;
        private IConnectionControl _connection;

        public override void LoadFromSource(int id) {
            ActionLoadTemplate action = new ActionLoadTemplate.Builder()
                .SetConnection(_connection)
                .SetTemplateId(id)
                .Build();
            _template.Data = action.PerformAction();
        }

        public override int SaveToSource() {
            ActionSaveTemplate action = new ActionSaveTemplate(_template, _connection);
            return action.PerformAction();
        }

        public class ActionLoadTemplate {
            private IConnectionControl _connection;
            private int _templateId;
            private Template.TemplateData _data = new Template.TemplateData();
            private StringBuilder _sql = new StringBuilder();
            private DataSet _dataSet;

            public Template.TemplateData PerformAction() {
                GenerateLoadSql();
                GetDataSetFromDatabase();
                PopulateDataFromDataSet();
                PopulateTemplateSlots();
                return _data;
            }

            private void GenerateLoadSql() {
                _sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Template");
                _sql.AppendLine("where TemplateId = " + _templateId);
                _sql.AppendLine("");
                _sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.TemplateSlot");
                _sql.AppendLine("where TemplateId = " + _templateId);
            }

            private void GetDataSetFromDatabase() {
                _dataSet = _connection.ExecuteDataSet(_sql.ToString());
            }

            private void PopulateDataFromDataSet() {
                _data.Id = _templateId;
                _data.Description = GetValueFromDataSet("Description").ToString();
                _data.IsDeleted = (bool)GetValueFromDataSet("IsDeleted");
            }

            private void PopulateTemplateSlots() {
                List<TemplateSlot> slots = new List<TemplateSlot>();
                foreach (DataRow dr in _dataSet.Tables[1].Rows) {
                    TemplateSlot slot = new TemplateSlot();
                    slot.Id = (int)dr["TemplateSlotId"];
                    slot.TemplateId = (int)dr["TemplateId"];
                    slot.SlotType = (Slot.enumSlotType)dr["SlotTypeId"];
                    slot.Title = dr["Title"].ToString();
                    slot.SortNumber = (int)dr["SortNumber"];                    
                    slots.Add(slot);
                }
                _data.TemplateSlots = slots;
            }

            private object GetValueFromDataSet(string columnName) {
                return _dataSet.Tables[0].Rows[0][columnName];
            }

            public ActionLoadTemplate(Builder builder) {
                _connection = builder.Connection;
                _templateId = builder.TemplateId;
            }

            public class Builder {
                public IConnectionControl Connection;
                public int TemplateId;

                public Builder SetConnection(IConnectionControl connection) {
                    this.Connection = connection;
                    return this;
                }

                public Builder SetTemplateId(int templateId) {
                    this.TemplateId = templateId;
                    return this;
                }

                public ActionLoadTemplate Build() {
                    return new ActionLoadTemplate(this);
                }
            }
        }

        public class ActionSaveTemplate {
            private IConnectionControl _connection;
            private ITemplate _template;
            private StringBuilder _sql = new StringBuilder();

            public int PerformAction() {
                PrepareSql();
                GenerateTableSql();
                GenerateTemplateSlotSql();
                AddNewIdReturn();
                CloseSQL();
                int id = PerformSave();
                return id;
            }

            private void PrepareSql() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionOpen());
                _sql.AppendLine("declare @TemplateId int");
            }

            private void GenerateTableSql() {
                if (_template.Id <= 0) {
                    GenerateTableSqlNew();
                } else {
                    GenerateTableSqlExisting();
                }
            }

            private void GenerateTableSqlNew() {
                _sql.AppendLine("insert into " + _connection.DatabaseName + ".dbo.Template(Description, IsDeleted)");
                _sql.Append("values('" + _connection.SqlSafe(_template.Description) + "'");
                _sql.Append("," + (_template.IsDeleted ? "1" : "0"));
                _sql.AppendLine(")");
                _sql.AppendLine("");
                _sql.AppendLine("select @TemplateId = max(TemplateId) from " + _connection.DatabaseName + ".dbo.Template");
            }

            private void GenerateTableSqlExisting() {
                _sql.AppendLine("");
                _sql.AppendLine("update " + _connection.DatabaseName + ".dbo.Template");
                _sql.AppendLine("set Description = '" + _connection.SqlSafe(_template.Description) + "'");
                _sql.AppendLine(", IsDeleted = " + (_template.IsDeleted ? "1" : "0"));
                _sql.AppendLine("where TemplateId = " + _template.Id);
                _sql.AppendLine("");
                _sql.AppendLine("set @TemplateId = " + _template.Id);
            }

            private void GenerateTemplateSlotSql() {
                DeleteExistingTemplateSlots();
                GenerateTemplateSlotSqlInidividuals();
            }

            private void DeleteExistingTemplateSlots() {
                _sql.AppendLine("");
                _sql.AppendLine("delete from " + _connection.DatabaseName + ".dbo.TemplateSlot");
                _sql.AppendLine("where TemplateId = " + _template.Id);
            }

            private void GenerateTemplateSlotSqlInidividuals() {
                _sql.AppendLine("");
                foreach (TemplateSlot slot in _template.TemplateSlots) {
                    _sql.AppendLine("insert into TemplateSlot(TemplateId, SlotTypeId, Title, SortNumber)");
                    _sql.Append("values(" + slot.TemplateId);
                    _sql.Append("," + (int)slot.SlotType);
                    _sql.Append(",'" + _connection.SqlSafe(slot.Title) + "'");
                    _sql.Append("," + slot.SortNumber);
                    _sql.AppendLine(")");
                }
            }

            private void AddNewIdReturn() {
                _sql.AppendLine("");
                _sql.AppendLine("select @TemplateId");
            }

            private void CloseSQL() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionClose());
            }

            private int PerformSave() {
                return (int)_connection.ExecuteScalar(_sql.ToString());
            }

            public ActionSaveTemplate(ITemplate template, IConnectionControl connection) {
                _template = template;
                _connection = connection;
            }
        }
        

        public TemplateDecoratorDatabase(Builder builder) : base(builder.Template) {
            _template = builder.Template;
            _connection = builder.Connection;
        }

        public class Builder {
            public ITemplate Template;
            public IConnectionControl Connection;

            public Builder SetTemplate(ITemplate template) {
                this.Template = template;
                return this;
            }

            public Builder SetConnection(IConnectionControl connection) {
                this.Connection = connection;
                return this;
            }

            public TemplateDecoratorDatabase Build() {
                return new TemplateDecoratorDatabase(this);
            }
        }
    }

    public class UserDecoratorDatabase : UserDecorator {
        private IUser _user;
        private IConnectionControl _connection;
        private IFactory _factory;

        public override void LoadFromSource(int id) {
            ActionLoadUser action = new ActionLoadUser.Builder()
                .SetConnection(_connection)
                .SetUserId(id)
                .Build();
            _user.Data = action.PerformAction();
        }

        public override int SaveToSource() {
            ActionSaveUser action = new ActionSaveUser(_connection, _user);
            return action.PerformAction();
        }

        public class ActionLoadUser {
            private IConnectionControl _connection;
            private IFactory _factory;
            private DataSet _dataSet;
            private int _userId;
            private User.UserData _data = new User.UserData();
            private StringBuilder _sql = new StringBuilder();

            public User.UserData PerformAction() {
                GenerateLoadSql();
                GetDataSetFromDatabase();
                PopulateDataFromDataSet();
                return _data;
            }

            private void GenerateLoadSql() {
                _sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.[User] U");
                _sql.AppendLine("left join " + _connection.DatabaseName + ".dbo.SlotFiller SL on SL.SlotFillerSourceId = U.UserId");
                _sql.AppendLine("where U.UserId = " + _userId);
            }

            private void GetDataSetFromDatabase() {
                _dataSet = _connection.ExecuteDataSet(_sql.ToString());
            }

            private void PopulateDataFromDataSet() {
                _data.Id = _userId;
                _data.Name = GetValueFromDataSet("Name").ToString();
                _data.SlotFillerId = (int)GetValueFromDataSet("SlotFillerId");
            }

            private object GetValueFromDataSet(string columnName) {
                return _dataSet.Tables[0].Rows[0][columnName];
            }

            public ActionLoadUser(Builder builder) {
                _factory = builder.Factory;
                _connection = builder.Connection;
                _userId = builder.UserId;
            }

            public class Builder {
                public IFactory Factory;
                public IConnectionControl Connection;
                public int UserId;

                public Builder SetFactory(IFactory factory) {
                    this.Factory = factory;
                    return this;
                }

                public Builder SetConnection(IConnectionControl connection) {
                    this.Connection = connection;
                    return this;
                }

                public Builder SetUserId(int userId) {
                    this.UserId = userId;
                    return this;
                }

                public ActionLoadUser Build() {
                    return new ActionLoadUser(this);
                }
            }
        }

        public class ActionSaveUser {
            private IConnectionControl _connection;
            private IUser _user;
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
                _sql.AppendLine("declare @UserId int");
                _sql.AppendLine("declare @SlotFillerId int");
            }

            private void GenerateTableSql() {
                if (_user.Id <= 0) {
                    GenerateTableSqlNew();
                } else {
                    GenerateTableSqlExisting();
                }
            }

            private void GenerateTableSqlNew() {
                _sql.AppendLine("select @SlotFillerId = max(SlotFillerId) from " + _connection.DatabaseName + ".dbo.SlotFiller");
                _sql.AppendLine("");
                _sql.AppendLine("insert into " + _connection.DatabaseName + ".dbo.[User](Name)");
                _sql.Append("values('" + _connection.SqlSafe(_user.Name) + "'");
                _sql.AppendLine(")");
                _sql.AppendLine("");
                _sql.AppendLine("select @UserId = max(UserId) from " + _connection.DatabaseName + ".dbo.[User]");
                _sql.AppendLine("");
                _sql.AppendLine("insert into " + _connection.DatabaseName + ".dbo.SlotFiller(SlotTypeId, SlotFillerSourceId)");
                _sql.Append("values(" + (int)Slot.enumSlotType.User);
                _sql.Append(",@UserId");
                _sql.AppendLine(")");
            }

            private void GenerateTableSqlExisting() {
                _sql.AppendLine("update " + _connection.DatabaseName + ".dbo.[User]");
                _sql.AppendLine("set Name = " + _connection.SqlSafe(_user.Name) + "'");
                _sql.AppendLine("where UserId = " + _user.Id);
                _sql.AppendLine("");
                _sql.AppendLine("set @UserId = " + _user.Id);
            }

            private void AddNewIdReturn() {
                _sql.AppendLine("");
                _sql.AppendLine("select @UserId");
            }

            private void CloseSQL() {
                _sql.AppendLine(_connection.GenerateSqlTryWithTransactionClose());
            }

            private int PerformSave() {
                return (int)_connection.ExecuteScalar(_sql.ToString());
            }

            public ActionSaveUser(IConnectionControl connection, IUser user) {
                _connection = connection;
                _user = user;
            }
        }

        public UserDecoratorDatabase(IUser user, IConnectionControl connection) : base(user) {
            _user = user;
            _connection = connection;
        }
    }

    public class FactoryDecoratorDatabase : FactoryDecorator {
        private IFactory _factory;
        private IConnectionControl _connection;

        public override ISlotFiller CreateSlotFiller(int id) {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select SlotTypeId, SlotFillerSourceId from " + _connection.DatabaseName + ".dbo.SlotFiller");
            sql.AppendLine("where SlotFillerId = " + id);
            DataSet ds = _connection.ExecuteDataSet(sql.ToString());
            Slot.enumSlotType slotType = (Slot.enumSlotType)ds.Tables[0].Rows[0]["SlotTypeId"];
            switch (slotType) {
                case Slot.enumSlotType.User:
                    IUser user = _factory.CreateUser();
                    user.LoadFromSource((int)ds.Tables[0].Rows[0]["SlotFillerSourceId"]);
                    return _factory.CreateUser().AsSlotFiller;
                case Slot.enumSlotType.None:
                    throw new Exception("Cannot cast 'None' to ISlotFiller interface");
                default:
                    throw new Exception(slotType.ToString() + " not implemeneted for FactoryDecoratorDatabase.CreateSlotFiller");
            }
        }

        public FactoryDecoratorDatabase(IFactory factory) : base(factory) {
            _factory = factory;
            _connection = _factory.CreateAppConnection(_factory.DatabaseInstance);
        }
    }

    public class EditTemplateViewDecoratorDatabase : EditTemplateViewDecorator {
        private IEditTemplateView _templateView;
        private IConnectionControl _connection;

        public override void LoadFromSource() {
            Dictionary<int, string> templates = new Dictionary<int, string>();
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Template where IsDeleted = 0");
            DataSet ds = _connection.ExecuteDataSet(sql.ToString());
            foreach (DataRow dr in ds.Tables[0].Rows) {
                templates.Add((int)dr["TemplateId"], dr["Description"].ToString());
            }
            _templateView.Templates = templates;
        }

        public EditTemplateViewDecoratorDatabase(IEditTemplateView templateView, IConnectionControl connection) : base(templateView) {
            _templateView = templateView;
            _connection = connection;
        }
    }

    public class MonthSelectorViewDecoratorDatabase : MonthSelectorViewDecorator {
        private IMonthSelectorView _monthSelector;
        private IConnectionControl _connection;
        private DateTime _currentMonth;
        private MonthSelectorView.MeetingsForMonth _meetingsForMonth;

        public override void LoadFromSource() {
            LoadTemplates();
        }

        public override MonthSelectorView.MeetingsForMonth BatonMeetings {
            get {
                if (_monthSelector.CurrentMonth != _currentMonth) {
                    LoadMeetings();
                    _currentMonth = _monthSelector.CurrentMonth;
                }
                return _meetingsForMonth;
            }
        }

        private void LoadTemplates() {
            Dictionary<int, string> templates = new Dictionary<int, string>();
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Template where IsDeleted = 0");
            DataSet ds = _connection.ExecuteDataSet(sql.ToString());
            foreach (DataRow dr in ds.Tables[0].Rows) {
                templates.Add((int)dr["TemplateId"], dr["Description"].ToString());
            }
            _monthSelector.Templates = templates;
        }

        private void LoadMeetings() {
            MonthSelectorView.MeetingsForMonth meetings = new MonthSelectorView.MeetingsForMonth();
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from " + _connection.DatabaseName + ".dbo.Meeting");
            sql.AppendLine("where year(MeetingDate) = year('" + _monthSelector.CurrentMonth.ToShortDateString() + "')");
            sql.AppendLine("and month(MeetingDate) = month('" + _monthSelector.CurrentMonth.ToShortDateString() + "')");
            sql.AppendLine("and IsDeleted = 0");
            DataSet ds = _connection.ExecuteDataSet(sql.ToString());
            foreach (DataRow dr in ds.Tables[0].Rows) {
                int meetingId = (int)dr["MeetingId"];
                int dayOfMonth = ((DateTime)dr["MeetingDate"]).Day;
                string meetingName = dr["MeetingDescription"].ToString();
                meetings.AddMeeting(meetingId, dayOfMonth, meetingName);
            }
            _meetingsForMonth = meetings;
        }

        public MonthSelectorViewDecoratorDatabase(IMonthSelectorView monthSelector, IConnectionControl connection) : base(monthSelector) {
            _monthSelector = monthSelector;
            _connection = connection;
        }
    }

    public class EditMeetingViewDecoratorDatabase : EditMeetingViewDecorator {
        private IEditMeetingView _meeting;
        private IConnectionControl _connection;

        public EditMeetingViewDecoratorDatabase(IEditMeetingView meeting, IConnectionControl connection) : base(meeting) {
            _meeting = meeting;
            _connection = connection;
        }
    }
}
