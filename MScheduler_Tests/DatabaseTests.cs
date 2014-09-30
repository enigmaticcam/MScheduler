using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;

namespace MScheduler_Tests {

    [TestClass]
    public class DatabaseTests {

        private string ConnectionString {
            get { return "Server=(localdb)\\v11.0;Integrated Security=true;initial catalog=MScheduler;"; }
        }

        private string DatabaseName {
            get { return "MScheduler"; }
        }

        private IConnectionControl GetConnection() {
            ConnectionControl connection = new ConnectionControl(this.DatabaseName, this.ConnectionString);
            return connection;
        }

        private void PrepForTesting() {
            IConnectionControl connection = GetConnection();
            TruncateTables(connection);
            AddTypes(connection);
        }

        private void TruncateTables(IConnectionControl connection) {
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.Meeting");
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.Slot");
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.SlotFiller");
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.SlotType");
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.Template");
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.TemplateSlot");
            connection.ExecuteNonQuery("truncate table " + connection.DatabaseName + ".dbo.[User]");
        }

        private void AddTypes(IConnectionControl connection) {
            AddSlotType(connection);
        }

        private void AddSlotType(IConnectionControl connection) {
            StringBuilder sql = new StringBuilder();
            foreach (Slot.enumSlotType slotType in Enum.GetValues(typeof(Slot.enumSlotType))) {
                sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.SlotType(SlotTypeId, Description)");
                sql.AppendLine("values(" + (int)slotType + ", '" + slotType.ToString() + "')");
            }
            connection.ExecuteNonQuery(sql.ToString());
        }

        [TestMethod]
        public void DatabaseTests_PrepDatabase() {

            // Arrange
            IConnectionControl connection = GetConnection();

            // Act
            PrepForTesting();
            int rowCountMeeting = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.Meeting");
            int rowCountSlot = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.Slot");
            int rowCountSlotFiller = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.SlotFiller");
            int rowCountSlotType = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.SlotType");
            int rowCountTemplate = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.Template");
            int rowCountTemplateSlot = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.TemplateSlot");
            int rowCountUser = (int)connection.ExecuteScalar("select Count(*) from " + connection.DatabaseName + ".dbo.[User]");

            // Assert
            Assert.AreEqual(0, rowCountMeeting);
            Assert.AreEqual(0, rowCountSlot);
            Assert.AreEqual(0, rowCountSlotFiller);
            Assert.AreEqual(Enum.GetValues(typeof(Slot.enumSlotType)).GetUpperBound(0) + 1, rowCountSlotType);
            Assert.AreEqual(0, rowCountTemplate);
            Assert.AreEqual(0, rowCountTemplateSlot);
            Assert.AreEqual(0, rowCountUser);
        }

        [TestMethod]
        public void DatabaseTests_MeetingSaveAndReload() {

            // Arrange
            IConnectionControl connection = GetConnection();

            Mock<ISlot> slot1 = new Mock<ISlot>();
            Mock<ISlot> slot2 = new Mock<ISlot>();
            slot1.Setup(s => s.Id).Returns(1);
            slot1.Setup(s => s.SortNumber).Returns(2);
            slot2.Setup(s => s.Id).Returns(2);
            slot2.Setup(s => s.SortNumber).Returns(1);
            List<ISlot> slots = new List<ISlot> { slot1.Object, slot2.Object };

            Mock<IFactory> factory = new Mock<IFactory>();
            int currentSlot = 0;
            factory.Setup(f => f.CreateSlot()).Returns(() => slots[currentSlot]).Callback(() => currentSlot++);

            Meeting.MeetingData data = new Meeting.MeetingData();
            data.Description = "MeetingDescription";
            data.Date = DateTime.Parse("5/23/2004");
            data.IsDeleted = true;
            data.Slots = slots;

            Mock<Meeting> meetingOld = new Mock<Meeting>();
            meetingOld.Object.Data = data;
            MeetingDecoratorDatabase database = new MeetingDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetFactory(factory.Object)
                .SetMeeting(meetingOld.Object)
                .Build();

            // Act
            PrepForTesting();
            CreateMockSlotsInDatabase(connection, 1);
            int meetingId = database.SaveToSource();
            Mock<Meeting> meetingNew = new Mock<Meeting>();
            database = new MeetingDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetFactory(factory.Object)
                .SetMeeting(meetingNew.Object)
                .Build();
            database.LoadFromSource(meetingId);

            // Assert
            Assert.AreEqual(meetingOld.Object.Description, meetingNew.Object.Description);
            Assert.AreEqual(meetingOld.Object.Date, meetingNew.Object.Date);
            Assert.AreEqual(meetingOld.Object.IsDeleted, meetingNew.Object.IsDeleted);
            Assert.AreEqual(meetingOld.Object.Slots.Count(), meetingNew.Object.Slots.Count());
            Assert.AreEqual(meetingOld.Object.Slots.ElementAt(0).Id, meetingNew.Object.Slots.ElementAt(0).Id);
            Assert.AreEqual(meetingOld.Object.Slots.ElementAt(1).Id, meetingNew.Object.Slots.ElementAt(1).Id);
        }

        private void CreateMockSlotsInDatabase(IConnectionControl connection, int meetingId) {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.Slot(MeetingId, SlotFillerId, Title, Description, SortNumber)");
            sql.AppendLine("values(" + meetingId + ", 0, 'Title', 'Description', 2)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.Slot(MeetingId, SlotFillerId, Title, Description, SortNumber)");
            sql.AppendLine("values(" + meetingId + ", 0, 'Title', 'Description', 1)");
            connection.ExecuteNonQuery(sql.ToString());
        }

        [TestMethod]
        public void DatabaseTests_SlotSaveAndReload() {

            // Arrange
            IConnectionControl connection = GetConnection();

            Slot.SlotData data = new Slot.SlotData();
            data.Description = "Description";
            data.SlotFillerId = 15;
            data.IsDeleted = false;
            data.MeetingId = 5;
            data.SortNumber = 10;
            data.Title = "Title";
            data.SlotType = Slot.enumSlotType.User;

            Mock<Slot> slotOld = new Mock<Slot>();
            slotOld.Object.Data = data;
            SlotDecoratorDatabase database = new SlotDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetSlot(slotOld.Object)
                .Build();

            // Act
            PrepForTesting();
            int slotId = database.SaveToSource();
            Mock<Slot> slotNew = new Mock<Slot>();
            database = new SlotDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetSlot(slotNew.Object)
                .Build();
            database.LoadFromSource(slotId);

            // Assert
            Assert.AreEqual(slotOld.Object.Description, slotNew.Object.Description);
            Assert.AreEqual(slotOld.Object.MeetingId, slotNew.Object.MeetingId);
            Assert.AreEqual(slotOld.Object.SlotFillerId, slotNew.Object.SlotFillerId);
            Assert.AreEqual(slotOld.Object.SortNumber, slotNew.Object.SortNumber);
            Assert.AreEqual(slotOld.Object.Title, slotNew.Object.Title);
            Assert.AreEqual(slotOld.Object.SlotType, slotNew.Object.SlotType);
        }

        [TestMethod]
        public void DatabaseTests_TemplateSaveAndReload() {

            // Arrange
            IConnectionControl connection = GetConnection();

            Mock<IFactory> factory = new Mock<IFactory>();

            TemplateSlot slot1 = new TemplateSlot();
            slot1.SlotType = Slot.enumSlotType.User;
            slot1.SortNumber = 2;
            slot1.TemplateId = 1;
            slot1.Title = "Title1";
            TemplateSlot slot2 = new TemplateSlot();
            slot2.SlotType = Slot.enumSlotType.User;
            slot2.SortNumber = 1;
            slot2.TemplateId = 1;
            slot2.Title = "Title2";
            List<TemplateSlot> slots = new List<TemplateSlot>();
            slots.Add(slot1);
            slots.Add(slot2);

            Template.TemplateData data = new Template.TemplateData();
            data.Description = "Description";
            data.TemplateSlots = slots;
            data.IsDeleted = true;

            Mock<Template> templateOld = new Mock<Template>(factory.Object);
            templateOld.Object.Data = data;
            TemplateDecoratorDatabase database = new TemplateDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetTemplate(templateOld.Object)
                .Build();

            // Act
            PrepForTesting();
            int templateId = database.SaveToSource();
            Mock<Template> templateNew = new Mock<Template>(factory.Object);
            database = new TemplateDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetTemplate(templateNew.Object)
                .Build();
            database.LoadFromSource(templateId);

            // Assert
            Assert.AreEqual(templateOld.Object.Description, templateNew.Object.Description);
            Assert.AreEqual(templateOld.Object.IsDeleted, templateNew.Object.IsDeleted);
            Assert.AreEqual(templateOld.Object.TemplateSlots.Count(), templateNew.Object.TemplateSlots.Count());
            Assert.AreEqual(templateOld.Object.TemplateSlots.ElementAt(0).SlotType, templateNew.Object.TemplateSlots.ElementAt(0).SlotType);
            Assert.AreEqual(templateOld.Object.TemplateSlots.ElementAt(0).SortNumber, templateNew.Object.TemplateSlots.ElementAt(0).SortNumber);
            Assert.AreEqual(templateOld.Object.TemplateSlots.ElementAt(0).Title, templateNew.Object.TemplateSlots.ElementAt(0).Title);
            Assert.AreEqual(templateOld.Object.TemplateSlots.ElementAt(1).SlotType, templateNew.Object.TemplateSlots.ElementAt(1).SlotType);
            Assert.AreEqual(templateOld.Object.TemplateSlots.ElementAt(1).SortNumber, templateNew.Object.TemplateSlots.ElementAt(1).SortNumber);
            Assert.AreEqual(templateOld.Object.TemplateSlots.ElementAt(1).Title, templateNew.Object.TemplateSlots.ElementAt(1).Title);
        }

        [TestMethod]
        public void DatabaseTests_UserSaveAndReload() {

            // Arrange
            IConnectionControl connection = GetConnection();

            User.UserData data = new User.UserData();
            data.Name = "Name";
            data.IsDeleted = true;

            Mock<User> userOld = new Mock<User>();
            userOld.Object.Data = data;
            UserDecoratorDatabase database = new UserDecoratorDatabase(userOld.Object, connection);
            
            // Act
            PrepForTesting();
            int userId = database.SaveToSource();
            Mock<User> userNew = new Mock<User>();
            database = new UserDecoratorDatabase(userNew.Object, connection);
            database.LoadFromSource(userId);

            // Assert
            Assert.AreEqual(userOld.Object.Description, userNew.Object.Description);
            Assert.AreEqual(userOld.Object.Name, userNew.Object.Name);
            Assert.AreEqual(userOld.Object.IsDeleted, userNew.Object.IsDeleted);
            Assert.AreNotEqual(0, userNew.Object.SlotFillerId);            
        }

        [TestMethod]
        public void DatabaseTests_TemplateProperlySavesWithDefaults() {

            // Arrange
            IConnectionControl connection = GetConnection();
            Mock<IFactory> factory = new Mock<IFactory>();

            Template template = new Template(factory.Object);
            TemplateDecoratorDatabase database = new TemplateDecoratorDatabase.Builder()
                .SetConnection(connection)
                .SetTemplate(template)
                .Build();

            // Act
            PrepForTesting();
            int templateId = database.SaveToSource();
            database.LoadFromSource(templateId);

            // Assert
            Assert.AreNotEqual(database.Description, null);
            Assert.AreNotEqual(database.Description, "");
        }

        [TestMethod]
        public void DatabaseTests_SlotFillerList_User() {

            // Arrange
            PrepForTesting();
            IConnectionControl connection = GetConnection();
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.[User](Name, IsDeleted)");
            sql.AppendLine("values('User1', 0)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.[User](Name, IsDeleted)");
            sql.AppendLine("values('User2', 0)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.[User](Name, IsDeleted)");
            sql.AppendLine("values('User3', 1)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.[User](Name, IsDeleted)");
            sql.AppendLine("values('User4', 0)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.SlotFiller(SlotTypeId, SlotFillerSourceId)");
            sql.AppendLine("values(1, 1)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.SlotFiller(SlotTypeId, SlotFillerSourceId)");
            sql.AppendLine("values(1, 2)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.SlotFiller(SlotTypeId, SlotFillerSourceId)");
            sql.AppendLine("values(1, 3)");
            sql.AppendLine("insert into " + connection.DatabaseName + ".dbo.SlotFiller(SlotTypeId, SlotFillerSourceId)");
            sql.AppendLine("values(2, 4)");
            connection.ExecuteNonQuery(sql.ToString());

            Mock<ISlotFiller> view = new Mock<ISlotFiller>();
            SlotFillerDecoratorDatabase database = new SlotFillerDecoratorDatabase(view.Object, connection);

            // Act            
            Dictionary<int, string> fillers = database.SlotFillersForType(Slot.enumSlotType.User);

            // Assert
            Assert.AreEqual(2, fillers.Count);
            Assert.AreEqual("User1", fillers[1]);
            Assert.AreEqual("User2", fillers[2]);
        }
    }
}
