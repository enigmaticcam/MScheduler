using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface IGroup {
        Group.Data GroupData { get; set; }
        int Id { get; }
        string GroupName { get; }
        Group.enumGroupType GroupType { get; }
        bool IsDeleted { get; }
        List<int> GroupObjects { get; }
    }

    public class Group : IGroup {
        public enum enumGroupType {
            User = 1,
            Slot,
            TemplateSlot
        }

        private Data _data;
        public Data GroupData {
            get { return _data; }
            set { _data = value; }
        }

        public int Id {
            get { return _data.GroupId; }
        }

        public string GroupName {
            get { return _data.GroupName; }
        }

        public enumGroupType GroupType {
            get { return _data.GroupType; }
        }

        public bool IsDeleted {
            get { return _data.IsDeleted; }
        }

        private HashSet<int> _groupObjects = new HashSet<int>();
        public List<int> GroupObjects {
            get { return _groupObjects.ToList(); }
        }

        public class Data {
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public enumGroupType GroupType { get; set; }
            public bool IsDeleted { get; set; }
        }
    }

    public abstract class GroupDecorator : IGroup {
        private IGroup _group;

        public virtual Group.Data GroupData {
            get { return _group.GroupData; }
            set { _group.GroupData = value; }
        }

        public virtual int Id {
            get { return _group.Id; }
        }

        public virtual string GroupName {
            get { return _group.GroupName; }
        }

        public virtual Group.enumGroupType GroupType {
            get { return _group.GroupType; }
        }

        public virtual bool IsDeleted {
            get { return _group.IsDeleted; }
        }

        public virtual List<int> GroupObjects {
            get { return _group.GroupObjects; }
        }

        public GroupDecorator(IGroup group) {
            _group = group;
        }
    }
}
