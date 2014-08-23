using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface ISlotFiller {
        int SlotFillerId { get; }
        int SlotFillerSourceId { get; }
        string Description { get; }
    }

    public interface ISlot {
        Slot.SlotData Data { set; }
        ISlotFiller Filler { get; }
        int Id { get; }
        int MeetingId { get; }
        string Title { get; }
        string Description { get; }
        int SortNumber { get; }
        int SlotFillerId { get; }
        bool CanFillSlot { get; }
        bool IsDeleted { get; }
        string FillSlot(ISlotFiller filler);
        void LoadFromSource(int id);
        int SaveToSource();
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

        public enum enumSlotType {
            None = 0,
            User
        }

        public ISlotFiller Filler {
            get { return _slotData.Filler; }
        }

        public int Id {
            get { return _slotData.Id; }
        }

        public int MeetingId {
            get { return _slotData.MeetingId; }
        }

        public string Title {
            get { return _slotData.Title; }
        }

        public string Description {
            get { return _slotData.Description; }
        }

        public int SortNumber {
            get { return _slotData.SortNumber; }
        }

        public int SlotFillerId {
            get {
                if (_slotData.Filler != null) {
                    return _slotData.Filler.SlotFillerId;
                } else {
                    return 0;
                }
            }
        }

        public bool CanFillSlot {
            get { return true; }
        }

        public bool IsDeleted {
            get { return _slotData.IsDeleted; }
        }

        public string FillSlot(ISlotFiller filler) {
            if (this.CanFillSlot) {
                _slotData.Filler = filler;
                return "";
            } else {
                return "This slot is unavailable";
            }
        }

        public void LoadFromSource(int id) {
            // To be implemented by decorator
        }

        public int SaveToSource() {
            // To be implemented by decorator
            return 0;
        }

        public class SlotData {
            public ISlotFiller Filler { get; set; }
            public int Id { get; set; }
            public int MeetingId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int SortNumber { get; set; }
            public bool IsDeleted { get; set; }
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

        public virtual int Id {
            get { return _slot.Id; }
        }

        public virtual int MeetingId {
            get { return _slot.MeetingId; }
        }

        public virtual string Title {
            get { return _slot.Title; }
        }

        public virtual string Description {
            get { return _slot.Description; }
        }

        public virtual int SortNumber {
            get { return _slot.SortNumber; }
        }

        public virtual bool CanFillSlot {
            get { return _slot.CanFillSlot; }
        }

        public virtual int SlotFillerId {
            get { return _slot.SlotFillerId; }
        }

        public virtual bool IsDeleted {
            get { return _slot.IsDeleted; }
        }

        public virtual string FillSlot(ISlotFiller filler) {
            return _slot.FillSlot(filler);
        }

        public virtual void LoadFromSource(int id) {
            _slot.LoadFromSource(id);
        }

        public virtual int SaveToSource() {
            return _slot.SaveToSource();
        }
    }
}
