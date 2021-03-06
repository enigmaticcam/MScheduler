﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public interface IEditTemplateView {
        Dictionary<int, string> Templates { get; set; }
        EditTemplateView.Baton DataBaton { get; set; }
        List<TemplateSlot> Slots { get; set; }
        int Id { get; }
        bool IsDeleted { get; }
        bool HasChanged { get; set; }
        string Message { get; set; }
        int CurrentSlotId { get; set; }
        int CreateTemplate();
        void SetTemplate(int templateId);
        void Save();
        void Delete(int id);        
        void LoadFromSource();
        void NewSlot(int id, List<TemplateSlot> slots);
    }

    public class EditTemplateView : IEditTemplateView {
        private IFactory _factory;
        private Template.TemplateData _data;

        private Dictionary<int, string> _templates;
        public Dictionary<int, string> Templates {
            get { return _templates; }
            set { _templates = value; }
        }

        public Baton DataBaton {
            get {
                Baton baton = new Baton();
                baton.Id = _data.Id;
                baton.Description = _data.Description;
                return baton;
            }
            set {
                _message = "";
                if (_data.Id != value.Id) {
                    _message = "Incorrect Template Id";
                } else {
                    _data.Description = value.Description;
                    _templates[_data.Id] = value.Description;
                    _hasChanged = true;
                }                
            }
        }

        public List<TemplateSlot> Slots {
            get {
                return _data.TemplateSlots.OrderBy(s => s.SortNumber).ToList(); 
            }
            set {
                AutoOrdererChangeCache<TemplateSlot> autoOrder = new AutoOrdererChangeCache<TemplateSlot>("SortNumber");
                foreach (TemplateSlot slot in value) {
                    int slotIndex = _data.TemplateSlots.FindIndex(s => s.Id == slot.Id);
                    if (_data.TemplateSlots[slotIndex].SortNumber != slot.SortNumber) {
                        autoOrder.AddSlotChange(slotIndex, _data.TemplateSlots[slotIndex].SortNumber, slot.SortNumber);
                    }
                }
                _data.TemplateSlots = value;
                autoOrder.PerformAutoSort(_data.TemplateSlots);
                _hasChanged = true;
            }
        }

        public int Id {
            get { return _data.Id; }
        }

        public bool IsDeleted {
            get { return _data.IsDeleted; }
        }

        private bool _hasChanged;
        public bool HasChanged {
            get { return _hasChanged; }
            set { _hasChanged = value; }
        }

        private string _message;
        public string Message {
            get { return _message; }
            set { _message = value; }
        }

        private int _currentSlotId;
        public int CurrentSlotId {
            get { return _currentSlotId; }
            set { _currentSlotId = value; }
        }

        public int CreateTemplate() {
            ITemplate template = _factory.CreateTemplate();
            int templateId = template.SaveToSource();
            _templates.Add(templateId, template.Description);
            return templateId;
        }

        public void SetTemplate(int templateId) {
            _message = "";
            if (_data == null || _data.Id != templateId) {
                ITemplate template = _factory.CreateTemplate();
                template.LoadFromSource(templateId);
                _data = template.Data;
                _hasChanged = false;
            }
        }

        public void Save() {
            if (_data == null) {
                _message = "Template has not been set";
            } else {
                ITemplate template = _factory.CreateTemplate();
                template.Data = _data;
                int templateId = template.SaveToSource();
                template.LoadFromSource(templateId);
                _data = template.Data;
                _hasChanged = false;
                _message = "Template Saved!";
            }
        }

        public void Delete(int id) {
            _message = "";
            if (_data.Id != id) {
                _message = "Template " + id + " has not been set";
            } else {
                _data.IsDeleted = !_data.IsDeleted;
                if (_data.IsDeleted) {
                    _message = "Template marked for deletion and will be deleted upon Save";
                } else {
                    _message = "Template marked for undeletion and will be undeleted upon Save";
                }
                _hasChanged = true;
            }
        }

        public void NewSlot(int id, List<TemplateSlot> slots) {
            _message = "";
            if (_data.Id != id) {
                _message = "Template " + id + " has not been set";
            } else {
                _data.TemplateSlots = slots;
                if (_data.TemplateSlots == null) {
                    _data.TemplateSlots = new List<TemplateSlot>();
                }
                TemplateSlot slot = new TemplateSlot();
                slot.TemplateId = _data.Id;
                AutoOrderer<TemplateSlot> autoOrder = new AutoOrderer<TemplateSlot>(_data.TemplateSlots, "SortNumber");
                slot.SortNumber = autoOrder.NextHighestId();
                _data.TemplateSlots.Add(slot);
                _hasChanged = true;
            }
        }

        public void LoadFromSource() {
            // To be implemented by decorators
        }

        public EditTemplateView(IFactory factory) {
            _factory = factory;
        }

        public class Baton {
            public int Id { get; set; }
            public string Description { get; set; }
        }

        public class BatonTemplateSlot {
            public int Id { get; set; }
            public int TemplateId { get; set; }
            public int SlotTypeId { get; set; }
            public string Title { get; set; }
            public int SortNumber { get; set; }
        }
    }

    public class EditTemplateViewDecorator : IEditTemplateView {
        private IEditTemplateView _templateView;

        public virtual Dictionary<int, string> Templates {
            get { return _templateView.Templates; }
            set { _templateView.Templates = value; }
        }        

        public virtual EditTemplateView.Baton DataBaton {
            get { return _templateView.DataBaton; }
            set { _templateView.DataBaton = value; }
        }

        public virtual List<TemplateSlot> Slots {
            get { return _templateView.Slots; }
            set { _templateView.Slots = value; }
        }

        public virtual int Id {
            get { return _templateView.Id; }
        }

        public virtual bool IsDeleted {
            get { return _templateView.IsDeleted; }
        }

        public virtual bool HasChanged {
            get { return _templateView.HasChanged; }
            set { _templateView.HasChanged = value; }
        }

        public virtual string Message {
            get { return _templateView.Message; }
            set { _templateView.Message = value; }
        }

        public virtual int CurrentSlotId {
            get { return _templateView.CurrentSlotId; }
            set { _templateView.CurrentSlotId = value; }
        }

        public virtual int CreateTemplate() {
            return _templateView.CreateTemplate();
        }

        public virtual void SetTemplate(int templateId) {
            _templateView.SetTemplate(templateId);
        }

        public virtual void Save() {
            _templateView.Save();
        }

        public virtual void Delete(int id) {
            _templateView.Delete(id);
        }

        public virtual void LoadFromSource() {
            _templateView.LoadFromSource();
        }

        public virtual void NewSlot(int id, List<TemplateSlot> slots) {
            _templateView.NewSlot(id, slots);
        }

        public EditTemplateViewDecorator(IEditTemplateView templateView) {
            _templateView = templateView;
        }
    }
}
