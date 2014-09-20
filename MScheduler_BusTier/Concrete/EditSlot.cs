using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MScheduler_BusTier.Concrete {
    public interface IEditSlot {

    }

    public class EditSlot : IEditSlot {

        public class Baton {

        }
    }

    public abstract class EditSlotDecorator : IEditSlot {
        private IEditSlot _editSlot;

        public EditSlotDecorator(IEditSlot editSlot) {
            _editSlot = editSlot;
        }
    }
}
