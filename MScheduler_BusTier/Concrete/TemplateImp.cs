using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MScheduler_BusTier.Abstract;

namespace MScheduler_BusTier.Concrete {
    public class TemplateImp : Template {
        public TemplateImp(IFactory factory) : base(factory) { }
    }
}
