﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;
using MScheduler_BusTier.Concrete;

namespace MScheduler_BusTier.Abstract {
    public interface IFactory {
        Factory.enumDatabaseInstance DatabaseInstance { get; }
        IConnectionControl CreateAppConnection(Factory.enumDatabaseInstance version);
        ISlot CreateSlot();
        IMeeting CreateMeeting();
        ITemplate CreateTemplate();
        ISlotFiller CreateSlotFiller();
        IUser CreateUser();
        IEditTemplateView CreateEditTemplateView();
        IEditMeetingView CreateEditMeetingView();
        IMonthSelectorView CreateMonthSelector();
    }

    public abstract class Factory : IFactory {
        public enum enumDatabaseInstance {
            Production = 0,
            Test,
            Development,
            LocalDB
        }
        public abstract enumDatabaseInstance DatabaseInstance { get; }
        public abstract IConnectionControl CreateAppConnection(Factory.enumDatabaseInstance version);
        public abstract ISlot CreateSlot();
        public abstract IMeeting CreateMeeting();
        public abstract ITemplate CreateTemplate();
        public abstract ISlotFiller CreateSlotFiller();
        public abstract IUser CreateUser();
        public abstract IEditTemplateView CreateEditTemplateView();
        public abstract IEditMeetingView CreateEditMeetingView();
        public abstract IMonthSelectorView CreateMonthSelector();

        public static IFactory CreateInstance() {
            Factory factory = new FactoryMain();
            return WrapInDecoratorsFactory(factory);
        }

        public static IFactory CreateInstance(enumDatabaseInstance databaseInsanceOverride) {
            Factory factory = new FactoryMain(databaseInsanceOverride);
            return WrapInDecoratorsFactory(factory);
        }

        private static IFactory WrapInDecoratorsFactory(Factory factory) {
            IFactory decoratoedFactory = factory;
            return decoratoedFactory;
        }
    }

    public abstract class FactoryDecorator : IFactory {
        private IFactory _factory;
        public FactoryDecorator(IFactory factory) {
            _factory = factory;
        }

        public virtual Factory.enumDatabaseInstance DatabaseInstance {
            get { return _factory.DatabaseInstance; }
        }

        public virtual IConnectionControl CreateAppConnection(Factory.enumDatabaseInstance version) {
            return _factory.CreateAppConnection(version);
        }

        public virtual ISlot CreateSlot() {
            return _factory.CreateSlot();
        }

        public virtual IMeeting CreateMeeting() {
            return _factory.CreateMeeting();
        }

        public virtual ITemplate CreateTemplate() {
            return _factory.CreateTemplate();
        }

        public virtual ISlotFiller CreateSlotFiller() {
            return _factory.CreateSlotFiller();
        }

        public virtual IUser CreateUser() {
            return _factory.CreateUser();
        }

        public virtual IEditTemplateView CreateEditTemplateView() {
            return _factory.CreateEditTemplateView();
        }

        public virtual IEditMeetingView CreateEditMeetingView() {
            return _factory.CreateEditMeetingView();
        }

        public virtual IMonthSelectorView CreateMonthSelector() {
            return _factory.CreateMonthSelector();
        }
    }
}
