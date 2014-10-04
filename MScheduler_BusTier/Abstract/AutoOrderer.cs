using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public class AutoOrderer<T> {
        private IEnumerable<T> _list;
        private List<T> _listOrdered;
        private string _idPropertyName;

        public int NextHighestId() {
            if (_list.Count() != 0) {
                RemoveSpacingBetweenSortIds();
                T highestItem = (
                    from i in _list
                    orderby GetIdForItem(i) descending
                    select i).First();
                return GetIdForItem(highestItem) + 1;
            } else {
                return 0;
            }            
        }

        public int NextLowestId() {
            if (_list.Count() != 0) {
                RemoveSpacingBetweenSortIds();
                T lowestItem = (
                    from i in _list
                    orderby GetIdForItem(i) ascending
                    select i).First();
                return GetIdForItem(lowestItem) - 1;
            } else {
                return -1;
            }            
        }

        public void ChangeIdAndReorder(int itemIndex, int newId) {
            RemoveSpacingBetweenSortIds();
            int oldId = GetIdForItem(_list.ElementAt(itemIndex));
            if (oldId > newId) {
                newId = Math.Max(newId, 0);
                IncreaseOrDecreaseRangeByOne(newId, oldId - 1, true);
            } else if (oldId < newId) {
                newId = Math.Min(newId, _list.Count() - 1);
                IncreaseOrDecreaseRangeByOne(oldId + 1, newId, false);
            }
            SetIdForItem(_list.ElementAt(itemIndex), newId);
        }

        private void IncreaseOrDecreaseRangeByOne(int startIndex, int endIndex, bool increase) {
            int step = 1;
            if (!increase) {
                step = -1;
            }
            for (int i = startIndex; i <= endIndex; i++) {
                T item = _listOrdered.ElementAt(i);
                SetIdForItem(item, GetIdForItem(item) + step);
            }
        }

        private void RemoveSpacingBetweenSortIds() {
            _listOrdered = _list.OrderBy(i => GetIdForItem(i)).ToList();
            int index = 0;
            foreach (T item in _listOrdered) {
                SetIdForItem(item, index);
                ++index;
            }
        }

        private int GetIdForItem(T item) {
            return Convert.ToInt32(item.GetType().GetProperty(_idPropertyName).GetValue(item, null));
        }

        private void SetIdForItem(T item, int value) {
            item.GetType().GetProperty(_idPropertyName).SetValue(item, value);
        }

        public AutoOrderer(IEnumerable<T> list, string idPropertyName) {
            _list = list;
            _idPropertyName = idPropertyName;
        }
    }
}