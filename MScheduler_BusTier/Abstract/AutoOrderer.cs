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

    public class AutoOrdererChangeCache<T> {
        private List<SortChange> _changes = new List<SortChange>();
        private string _idPropertyName;

        public void AddSlotChange(int itemIndex, int oldSort, int newSort) {
            SortChange change = new SortChange();
            change.ItemIndex = itemIndex;
            change.OldSort = oldSort;
            change.NewSort = newSort;
            _changes.Add(change);
        }

        public void PerformAutoSort(List<T> items) {
            foreach (SortChange change in _changes) {
                AutoOrderer<T> orderer = new AutoOrderer<T>(items, _idPropertyName);
                orderer.ChangeIdAndReorder(change.ItemIndex, change.NewSort);
            }
        }

        private int GetIdForItem(T item) {
            return Convert.ToInt32(item.GetType().GetProperty(_idPropertyName).GetValue(item, null));
        }

        public AutoOrdererChangeCache(string idPropertyName) {
            _idPropertyName = idPropertyName;
        }

        private class SortChange {
            public int ItemIndex { get; set; }
            public int OldSort { get; set; }
            public int NewSort { get; set; }
        }
    }
}