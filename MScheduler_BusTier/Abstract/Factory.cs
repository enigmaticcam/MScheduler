using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Abstract {
    public interface IFactory {
        ISlot CreateSlot();
        IMeeting CreateMeeting();
        ITemplate CreateTemplate();
        ISlotFiller CreateSlotFiller();
    }

    public abstract class Factory {
        public abstract ISlot CreateSlot();
        public abstract IMeeting CreateMeeting();
        public abstract ITemplate CreateTemplate();
        public abstract ISlotFiller CreateSlotFiller();
    }

    public abstract class FactoryDecorator {
        private IFactory _factory;
        public FactoryDecorator(IFactory factory) {
            _factory = factory;
        }

        public virtual ISlot CreateSlot() {
            return _factory.CreateSlot();
        }

        public virtual IMeeting CreateMeeting() {
            return _factory.CreateMeeting();
        }

        public virtual ITemplate CreateTemplate() {
            return _factory.CreateTemplate();
        }

        public virtual ISlotFiller CreateSlotFiller() {
            return _factory.CreateSlotFiller();
        }
    }
}
