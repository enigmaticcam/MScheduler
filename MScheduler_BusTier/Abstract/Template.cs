using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Abstract {
    public interface ITemplate {
        Template.TemplateData Data { get; set; }
        int Id { get; }
        string Description { get; }
        bool IsDeleted { get; set; }
        IEnumerable<TemplateSlot> TemplateSlots { get; }
        List<ISlot> GenerateMeetingSlots();
        void LoadFromSource(int id);
        int SaveToSource();
    }

    public class Template : ITemplate {
        private IFactory _factory;

        private TemplateData _templateData;
        public TemplateData Data {
            get { return _templateData; }
            set { _templateData = value; }
        }

        public Template(IFactory factory) {
            _factory = factory;
            CreateDefaults();
        }

        public int Id {
            get { return _templateData.Id; }
        }

        public string Description {
            get { return _templateData.Description; }
        }

        public bool IsDeleted {
            get { return _templateData.IsDeleted; }
            set { _templateData.IsDeleted = value; }
        }

        public IEnumerable<TemplateSlot> TemplateSlots {
            get { return _templateData.TemplateSlots.OrderBy(t => t.SortNumber); }
        }

        public List<ISlot> GenerateMeetingSlots() {
            ActionGenerateMeetingSlots action = new ActionGenerateMeetingSlots.Builder()
                .SetFactory(_factory)
                .SetTemplateSlots(_templateData.TemplateSlots)
                .Build();
            return action.PerformAction();
        }

        private void CreateDefaults() {
            _templateData = new TemplateData();
            _templateData.Description = "New Template";
            _templateData.TemplateSlots = new List<TemplateSlot>();
        }

        public void LoadFromSource(int id) {
            // To be implemented by decorator
        }

        public int SaveToSource() {
            // To be imeplemented by decorator
            return 0;
        }

        public class ActionGenerateMeetingSlots {
            private List<TemplateSlot> _templateSlots;
            private IFactory _factory;
            private List<ISlot> _slots;

            public List<ISlot> PerformAction() {
                _slots = new List<ISlot>();
                LoopThroughTemplateSlotsAndGenerate();
                return _slots;
            }

            private void LoopThroughTemplateSlotsAndGenerate() {
                foreach (TemplateSlot templateSlot in _templateSlots.OrderBy(t => t.SortNumber)) {
                    _slots.Add(GenerateSlotFromTemplateSlot(templateSlot));
                }
            }

            private ISlot GenerateSlotFromTemplateSlot(TemplateSlot templateSlot) {
                Slot.SlotData slotData = CopyTemplateSlotDataToSlot(templateSlot);
                ISlot slot = CreateSlotWithSlotData(slotData);
                return slot;
            }

            private Slot.SlotData CopyTemplateSlotDataToSlot(TemplateSlot templateSlot) {
                Slot.SlotData slotData = new Slot.SlotData();
                slotData.Title = templateSlot.Title;
                slotData.SortNumber = templateSlot.SortNumber;
                slotData.SlotType = templateSlot.SlotType;
                return slotData;
            }

            private ISlot CreateSlotWithSlotData(Slot.SlotData slotData) {
                ISlot slot = _factory.CreateSlot();
                slot.Data = slotData;
                return slot;
            }

            public ActionGenerateMeetingSlots(Builder builder) {
                _templateSlots = builder.TemplateSlots;
                _factory = builder.Factory;
            }

            public class Builder {
                public List<TemplateSlot> TemplateSlots;
                public IFactory Factory;

                public Builder SetTemplateSlots(List<TemplateSlot> templateSlots) {
                    this.TemplateSlots = templateSlots;
                    return this;
                }

                public Builder SetFactory(IFactory factory) {
                    this.Factory = factory;
                    return this;
                }

                public ActionGenerateMeetingSlots Build() {
                    return new ActionGenerateMeetingSlots(this);
                }
            }
        }

        public class TemplateData {
            public int Id { get; set; }
            public string Description { get; set; }
            public bool IsDeleted { get; set; }
            public List<TemplateSlot> TemplateSlots { get; set; }
        }        
    }

    public class TemplateSlot {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Slot.enumSlotType SlotType { get; set; }
        public string Title { get; set; }
        public int SortNumber { get; set; }
    }

    public class TemplateDecorator : ITemplate {
        private ITemplate _template;
        public TemplateDecorator(ITemplate template) {
            _template = template;
        }

        public virtual Template.TemplateData Data {
            get { return _template.Data; }
            set { _template.Data = value; }
        }

        public virtual int Id {
            get { return _template.Id; }
        }
        
        public virtual string Description {
            get { return _template.Description; }
        }

        public virtual bool IsDeleted {
            get { return _template.IsDeleted; }
            set { _template.IsDeleted = value; }
        }

        public virtual IEnumerable<TemplateSlot> TemplateSlots {
            get { return _template.TemplateSlots; }
        }

        public virtual List<ISlot> GenerateMeetingSlots() {
            return _template.GenerateMeetingSlots();
        }

        public virtual void LoadFromSource(int id) {
            _template.LoadFromSource(id);
        }

        public virtual int SaveToSource() {
            return _template.SaveToSource();
        }
    }
}
