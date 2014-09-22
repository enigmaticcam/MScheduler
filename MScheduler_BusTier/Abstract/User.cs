using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface IUser {
        User.UserData Data { set; }
        int Id { get; }
        int SlotFillerId { get; }
        int SlotFillerSourceId { get; }
        string Description { get; }
        string Name { get; }
        void LoadFromSource(int id);
        int SaveToSource();
    }

    public class User : IUser {
        private UserData _data;

        public UserData Data {
            set { _data = value; }
        }

        public int Id {
            get { return _data.Id; }
        }

        public int SlotFillerId {
            get { return _data.SlotFillerId; }
        }

        public int SlotFillerSourceId {
            get { return _data.Id; }
        }

        public string Name {
            get { return _data.Name; }
        }

        public string Description {
            get { return this.Name; }
        }

        public void LoadFromSource(int id) {
            // To be implemented by decorator
        }

        public int SaveToSource() {
            // To be implemented by decorator
            return 0;
        }

        public User() {
            _data = new UserData();
        }

        public User(UserData data) {
            _data = data;
        }

        public class UserData {
            public int Id { get; set; }
            public int SlotFillerId { get; set; }
            public string Name { get; set; }
        }
    }

    public abstract class UserDecorator : IUser {
        private IUser _user;

        public virtual User.UserData Data {
            set { _user.Data = value; }
        }

        public virtual int Id {
            get { return _user.Id; }
        }

        public virtual int SlotFillerId {
            get { return _user.SlotFillerId; }
        }

        public virtual int SlotFillerSourceId {
            get { return _user.SlotFillerSourceId; }
        }

        public virtual string Name {
            get { return _user.Name; }
        }

        public virtual string Description {
            get { return _user.Description; }
        }

        public UserDecorator(IUser user) {
            _user = user;
        }

        public virtual void LoadFromSource(int id) {
            _user.LoadFromSource(id);
        }

        public virtual int SaveToSource() {
            return _user.SaveToSource();
        }
    }
}
