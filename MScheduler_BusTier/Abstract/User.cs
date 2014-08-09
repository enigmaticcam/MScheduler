﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Abstract {
    public interface IUser {
        User.UserData Data { set; }
        int Id { get; }        
        string Name { get; }
    }

    public class User : IUser, ISlotFiller {
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

        public string Name {
            get { return _data.Name; }
        }

        public string Description {
            get { return this.Name; }
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

        public virtual string Name {
            get { return _user.Name; }
        }

        public UserDecorator(IUser user) {
            _user = user;
        }
    }
}