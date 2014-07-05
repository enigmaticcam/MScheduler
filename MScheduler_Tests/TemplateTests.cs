using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;
using Moq;

namespace MScheduler_Tests {

    [TestClass]
    public class TemplateTests {

        [TestMethod]
        public void TemplateTests_GenerateMeetingSlots() {

            // Arrange
            Mock<IFactory> factory = new Mock<IFactory>();
            factory.Setup(f => f.CreateSlot()).Returns(new SlotImp());

            List<TemplateSlot> templateSlots = new List<TemplateSlot>();

            TemplateSlot templateSlot = new TemplateSlot();
            templateSlot.SortNumber = 5;
            

            Template.ActionGenerateMeetingSlots action = new Template.ActionGenerateMeetingSlots.Builder()
                .SetFactory(factory.Object)
                .SetTemplateSlots(null)
                .Build();
                
        }
    }
}
