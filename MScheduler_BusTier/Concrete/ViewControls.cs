using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Concrete {
    public class SelectionItem {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsSelected { get; set; }

        public SelectionItem() {

        }

        public SelectionItem(string text, string value) {
            this.Text = text;
            this.Value = value;
        }

        public SelectionItem(string text, string value, bool isDisabled) {
            this.Text = text;
            this.Value = value;
            this.IsDisabled = isDisabled;
        }

        public SelectionItem(string text, string value, bool isDisabled, bool isSelected) {
            this.Text = text;
            this.Value = value;
            this.IsDisabled = isDisabled;
            this.IsSelected = isSelected;
        }
    }
}
