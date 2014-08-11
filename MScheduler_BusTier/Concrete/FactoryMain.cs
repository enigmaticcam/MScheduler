using System;
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
                    connection = new ConnectionControl("MScheduler", "Server=(localdb)\\v11.0;Integrated Security=true;initial catalog=DynamicETL;");
                    break;
                default:
                    throw new Exception("Database Instance Connection not implemented");
            }
            return connection;
        }

        public override IMeeting CreateMeeting() {
            throw new NotImplementedException();
        }

        public override ISlot CreateSlot() {
            throw new NotImplementedException();
        }

        public override ISlotFiller CreateSlotFiller(int id) {
            throw new NotImplementedException();
        }

        public override ITemplate CreateTemplate() {
            throw new NotImplementedException();
        }
    }
}
