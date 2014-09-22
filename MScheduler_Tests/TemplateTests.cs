using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;
using Moq;

namespace MScheduler_Tests {

    [TestClass]
    public class TemplateTests {

        [TestMethod]
        public void TemplateTests_CarryAllAttributesFromTemplateSlotToSlot() {

            // Arrange
            List<Slot> returnSlots = new List<Slot>();
            returnSlots.Add(new Slot());
            returnSlots.Add(new Slot());

            int currentSlot = 0;
            Mock<IFactory> factory = new Mock<IFactory>();
            factory.Setup(f => f.CreateSlot()).Returns(() => returnSlots[currentSlot]).Callback(() => currentSlot++);

            List<TemplateSlot> templateSlots = new List<TemplateSlot>();

            TemplateSlot templateSlot = new TemplateSlot();
            templateSlot.Title = "Title1";
            templateSlot.SortNumber = 5;
            templateSlot.SlotType = Slot.enumSlotType.None;
            templateSlots.Add(templateSlot);

            templateSlot = new TemplateSlot();
            templateSlot.Title = "Title2";
            templateSlot.Id = 2;
            templateSlot.SortNumber = 3;
            templateSlot.SlotType = Slot.enumSlotType.User;
            templateSlots.Add(templateSlot);

            Template.TemplateData data = new Template.TemplateData();
            data.TemplateSlots = templateSlots;

            Mock<Template> template = new Mock<Template>(factory.Object);
            template.Object.Data = data;

            // Act
            List<ISlot> slots = template.Object.GenerateMeetingSlots().OrderBy(s => s.SortNumber).ToList();
            
            // Assert
            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(templateSlots[1].Title, slots[0].Title);
            Assert.AreEqual(templateSlots[0].Title, slots[1].Title);
            Assert.AreEqual(templateSlots[1].SortNumber, slots[0].SortNumber);
            Assert.AreEqual(templateSlots[0].SortNumber, slots[1].SortNumber);
            Assert.AreEqual(templateSlots[1].SlotType, slots[0].SlotType);
            Assert.AreEqual(templateSlots[0].SlotType, slots[1].SlotType);
        }
    }
}
