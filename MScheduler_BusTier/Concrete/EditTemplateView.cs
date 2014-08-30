using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public interface IEditTemplateView {
        Dictionary<int, string> Templates { get; set; }
        void LoadFromSource();
    }

    public class EditTemplateView : IEditTemplateView {
        private IFactory _factory;

        private Dictionary<int, string> _templates;
        public Dictionary<int, string> Templates {
            get { return _templates; }
            set { _templates = value; }
        }

        public void LoadFromSource() {
            // To be implemented by decorators
        }

        public EditTemplateView(IFactory factory) {
            _factory = factory;
        }        
    }

    public class EditTemplateViewDecorator : IEditTemplateView {
        private IEditTemplateView _templateView;

        public virtual Dictionary<int, string> Templates {
            get { return _templateView.Templates; }
            set { _templateView.Templates = value; }
        }

        public virtual void LoadFromSource() {
            _templateView.LoadFromSource();
        }

        public EditTemplateViewDecorator(IEditTemplateView templateView) {
            _templateView = templateView;
        }
    }
}
