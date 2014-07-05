using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface ISlotFiller {
        int Id { get; }
        string Description { get; }
    }

    public interface ISlot {
        Slot.SlotData Data { set; }
        ISlotFiller Filler { get; }
        string Description { get; }
        int SortNumber { get; }
    }

    public abstract class Slot : ISlot {
        private SlotData _slotData;
        protected SlotData PrivateData {
            get { return _slotData; }
            set { _slotData = value; }
        }
        public SlotData Data {
            set { _slotData = value; }
        }

        public enum SlotType {
            User = 0
        }

        public ISlotFiller Filler {
            get { return _slotData.Filler; }
        }

        public string Description {
            get { return _slotData.Description; }
        }

        public int SortNumber {
            get { return _slotData.SortNumber; }
        }

        public class SlotData {
            public ISlotFiller Filler { get; set; }
            public string Description { get; set; }
            public int SortNumber { get; set; }
        }
    }

    public abstract class SlotDecorator : ISlot {
        private ISlot _slot;
        public SlotDecorator(ISlot slot) {
            _slot = slot;
        }

        public virtual Slot.SlotData Data {
            set { _slot.Data = value; }
        }

        public virtual ISlotFiller Filler {
            get { return _slot.Filler; }
        }

        public virtual string Description {
            get { return _slot.Description; }
        }

        public virtual int SortNumber {
            get { return _slot.SortNumber; }
        }
    }
}
