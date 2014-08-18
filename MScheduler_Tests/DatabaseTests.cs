using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MScheduler_BusTier.Abstract;

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
    }
}
