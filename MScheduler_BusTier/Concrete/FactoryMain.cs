using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public class FactoryMain : Factory {

        private enumDatabaseInstance DefaultDatabaseInstance {
            get { return enumDatabaseInstance.LocalDB; }
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

        public override ISlotFiller CreateSlotFiller(int id) {
            // To be implemented in database decorator
            return null;
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

        private IMeeting WrapInDecoratorsMeeting(Meeting meeting) {
            IMeeting decoratedMeeting = new MeetingDecoratorDatabase.Builder()
                .SetConnection(this.CreateAppConnection(this.DefaultDatabaseInstance))
                .SetFactory(this)
                .SetMeeting(meeting)
                .Build();
            return decoratedMeeting;
        }

        private ISlot WrapInDecoratorsSlot(Slot slot) {
            ISlot decoratedSlot = new SlotDecoratorDatabase.Builder()
                .SetConnection(this.CreateAppConnection(this.DefaultDatabaseInstance))
                .SetFactory(this)
                .SetSlot(slot)
                .Build();
            return decoratedSlot;
        }

        private IUser WrapInDecoratorsUser(User user) {
            IUser decoratoedUser = new UserDecoratorDatabase(user, this.CreateAppConnection(this.DefaultDatabaseInstance));
            return decoratoedUser;
        }

        private ITemplate WrapInDecoratorsTemplate(Template template) {
            ITemplate decoratedTemplate = new TemplateDecoratorDatabase.Builder()
                .SetConnection(this.CreateAppConnection(this.DefaultDatabaseInstance))
                .SetTemplate(template)
                .Build();
            return decoratedTemplate;
        }

        private IEditTemplateView WrapInDecoratorsEditTemplateView(EditTemplateView templateView) {
            IEditTemplateView decoratedTemplateView = new EditTemplateViewDecoratorDatabase(templateView, this.CreateAppConnection(this.DefaultDatabaseInstance));
            return decoratedTemplateView;
        }
    }
}
