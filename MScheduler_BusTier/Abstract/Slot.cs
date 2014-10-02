using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface ISlotFiller {
        Dictionary<int, string> SlotFillersForType(Slot.enumSlotType slotType);
    }

    public interface ISlot {
        Slot.SlotData Data { set; }
        Slot.enumSlotType SlotType { get; }
        int Id { get; }
        int MeetingId { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        int SortNumber { get; set; }
        int SlotFillerId { get; }
        bool CanFillSlot { get; }
        bool IsDeleted { get; }
        void FillSlot(int slotFillerId);
        void LoadFromSource(int id);
        int SaveToSource();
    }

    public class SlotFiller : ISlotFiller {
        public Dictionary<int, string> SlotFillersForType(Slot.enumSlotType slotType) {
            return null;
        }        
    }

    public class Slot : ISlot {
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

        public Slot.enumSlotType SlotType {
            get { return _slotData.SlotType; }
        }

        public int Id {
            get { return _slotData.Id; }            
        }

        public int MeetingId {
            get { return _slotData.MeetingId; }
            set { _slotData.MeetingId = value; }
        }

        public string Title {
            get { return _slotData.Title; }
            set { _slotData.Title = value; }
        }

        public string Description {
            get { return _slotData.Description; }
            set { _slotData.Description = value; }
        }

        public int SortNumber {
            get { return _slotData.SortNumber; }
            set { _slotData.SortNumber = value; }
        }

        public int SlotFillerId {
            get { return _slotData.SlotFillerId; }
        }

        public bool CanFillSlot {
            get { return true; }
        }

        public bool IsDeleted {
            get { return _slotData.IsDeleted; }
        }

        public void FillSlot(int slotFillerId) {
            if (this.CanFillSlot) {
                _slotData.SlotFillerId = slotFillerId;
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
            public int SlotFillerId { get; set; }
            public enumSlotType SlotType { get; set; }
            public int Id { get; set; }
            public int MeetingId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int SortNumber { get; set; }
            public bool IsDeleted { get; set; }
        }
    }

    public abstract class SlotFillerDecorator : ISlotFiller {
        private ISlotFiller _slotFiller;

        public virtual Dictionary<int, string> SlotFillersForType(Slot.enumSlotType slotType) {
            return _slotFiller.SlotFillersForType(slotType);
        }

        public SlotFillerDecorator(ISlotFiller slotFiller) {
            _slotFiller = slotFiller;
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

        public virtual Slot.enumSlotType SlotType {
            get { return _slot.SlotType; }
        }

        public virtual int Id {
            get { return _slot.Id; }
        }

        public virtual int MeetingId {
            get { return _slot.MeetingId; }
            set { _slot.MeetingId = value; }
        }

        public virtual string Title {
            get { return _slot.Title; }
            set { _slot.Title = value; }
        }

        public virtual string Description {
            get { return _slot.Description; }
            set { _slot.Description = value; }
        }

        public virtual int SortNumber {
            get { return _slot.SortNumber; }
            set { _slot.SortNumber = value; }
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

        public void FillSlot(int slotFillerId) {
            _slot.FillSlot(slotFillerId);
        }

        public virtual void LoadFromSource(int id) {
            _slot.LoadFromSource(id);
        }

        public virtual int SaveToSource() {
            return _slot.SaveToSource();
        }
    }
}
