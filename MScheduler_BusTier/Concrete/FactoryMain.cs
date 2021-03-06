﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public class FactoryMain : Factory {

        private enumDatabaseInstance DefaultDatabaseInstance {
            get { return enumDatabaseInstance.Development; }
        }

        public FactoryMain() {
            _databaseInstance = this.DefaultDatabaseInstance;
        }

        public FactoryMain(enumDatabaseInstance databaseInstanceOverride) {
            _databaseInstance = databaseInstanceOverride;
        }

        private Factory.enumDatabaseInstance _databaseInstance;
        public override Factory.enumDatabaseInstance DatabaseInstance {
            get { return _databaseInstance; }
        }

        public override IConnectionControl CreateAppConnection(enumDatabaseInstance version) {
            IConnectionControl connection;
            switch (version) {
                case enumDatabaseInstance.LocalDB:
                    connection = new ConnectionControl("MScheduler", "Server=(localdb)\\v11.0;Integrated Security=true;initial catalog=MScheduler;");
                    break;
                case enumDatabaseInstance.Development:
                    connection = new ConnectionControl("MScheduler", "Server=SUPERFLY\\SQLEXPRESS;Integrated Security=true;initial catalog=MScheduler;");
                    break;
                default:
                    throw new Exception("Database Instance Connection not implemented");
            }
            return connection;
        }

        public override IMeeting CreateMeeting() {
            Meeting meeting = new Meeting();
            return WrapInDecoratorsMeeting(meeting);
        }

        public override ISlot CreateSlot() {
            Slot slot = new Slot();
            return WrapInDecoratorsSlot(slot);
        }

        public override ISlotFiller CreateSlotFiller() {
            SlotFiller slotFiller = new SlotFiller();
            return WrapInDecoratorsSlotFiller(slotFiller);
        }

        public override ITemplate CreateTemplate() {
            Template template = new Template(this);
            return WrapInDecoratorsTemplate(template);
        }

        public override IUser CreateUser() {
            User user = new User();
            return WrapInDecoratorsUser(user);
        }

        public override IEditTemplateView CreateEditTemplateView() {
            EditTemplateView templateView = new EditTemplateView(this);
            return WrapInDecoratorsEditTemplateView(templateView);
        }

        public override IEditMeetingView CreateEditMeetingView() {
            EditMeetingView meetingView = new EditMeetingView(this, this.CreateSlotFiller());
            return WrapInDecoratorsEditMeetingView(meetingView);
        }

        public override IMonthSelectorView CreateMonthSelector() {
            MonthSelectorView monthSelector = new MonthSelectorView();
            return WrapInDecoratorsMonthSelectorView(monthSelector);
        }

        private IMeeting WrapInDecoratorsMeeting(Meeting meeting) {
            IMeeting decoratedMeeting = meeting;
            decoratedMeeting = new MeetingDecoratorDatabase.Builder()
                .SetConnection(this.CreateAppConnection(this.DatabaseInstance))
                .SetFactory(this)
                .SetMeeting(decoratedMeeting)
                .Build();
            return decoratedMeeting;
        }

        private ISlot WrapInDecoratorsSlot(Slot slot) {
            ISlot decoratedSlot = slot;
            decoratedSlot = new SlotDecoratorDatabase.Builder()
                .SetConnection(this.CreateAppConnection(this.DatabaseInstance))
                .SetFactory(this)
                .SetSlot(decoratedSlot)
                .Build();
            return decoratedSlot;
        }

        private IUser WrapInDecoratorsUser(User user) {
            IUser decoratoedUser = user;
            decoratoedUser = new UserDecoratorDatabase(decoratoedUser, this.CreateAppConnection(this.DatabaseInstance));
            return decoratoedUser;
        }

        private ITemplate WrapInDecoratorsTemplate(Template template) {
            ITemplate decoratedTemplate = template;
            decoratedTemplate = new TemplateDecoratorDatabase.Builder()
                .SetConnection(this.CreateAppConnection(this.DatabaseInstance))
                .SetTemplate(decoratedTemplate)
                .Build();
            return decoratedTemplate;
        }

        private IEditTemplateView WrapInDecoratorsEditTemplateView(EditTemplateView templateView) {
            IEditTemplateView decoratedTemplateView = templateView;
            decoratedTemplateView = new EditTemplateViewDecoratorDatabase(decoratedTemplateView, this.CreateAppConnection(this.DatabaseInstance));
            return decoratedTemplateView;
        }

        private IEditMeetingView WrapInDecoratorsEditMeetingView(EditMeetingView meetingView) {
            IEditMeetingView decoratedMeetingView = meetingView;
            decoratedMeetingView = new EditMeetingViewDecoratorDatabase(decoratedMeetingView, this.CreateAppConnection(this.DatabaseInstance));
            return decoratedMeetingView;
        }

        private IMonthSelectorView WrapInDecoratorsMonthSelectorView(MonthSelectorView monthSelector) {
            IMonthSelectorView decorated = monthSelector;
            decorated = new MonthSelectorViewDecoratorDatabase(decorated, this.CreateAppConnection(this.DatabaseInstance));
            return decorated;
        }

        private ISlotFiller WrapInDecoratorsSlotFiller(SlotFiller slotFiller) {
            ISlotFiller decorated = slotFiller;
            decorated = new SlotFillerDecoratorDatabase(decorated, this.CreateAppConnection(this.DatabaseInstance));
            return decorated;
        }
    }
}
