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

        public static List<SelectionItem> ConvertEnumToSelectionItem<TEnum>(TEnum selectedValue) {
            List<SelectionItem> items = new List<SelectionItem>();
            string[] keys = Enum.GetNames(typeof(TEnum));
            Array values = Enum.GetValues(typeof(TEnum));
            for (int i = 0; i <= keys.GetUpperBound(0); i++) {
                SelectionItem item = new SelectionItem(keys[i], ((int)values.GetValue(i)).ToString());
                if (item.Text == selectedValue.ToString()) {
                    item.IsSelected = true;
                }
                items.Add(item);
            }
            return items;
        }
    }

    public class MessageCenter {
        private Dictionary<string, string> _messages = new Dictionary<string, string>();
        public string Message {
            get {
                StringBuilder message = new StringBuilder();
                foreach (string key in _messages.Keys.ToList()) {
                    message.AppendLine(_messages[key]);
                }
                _messages = new Dictionary<string, string>();
                return message.ToString();
            }
        }

        public void AddMessage(string messageId, string message) {
            if (_messages.ContainsKey(messageId)) {
                _messages[messageId] = message;
            } else {
                _messages.Add(messageId, message);
            }
        }
    }
}
