using RemoteAnalyst.TransMon.FactoryPatternExample.Controller;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Repository;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Service;

namespace RemoteAnalyst.TransMon.TransMonFactoryPattern.Controller {
    internal class TransMonFactory : AbstractFactory {
        public override AbstractService CreateServiceObj(IRepository iRepository) {
            return new TimestampService(iRepository);
        }

        public override AbstractRepository CreateRepositoryObj() {
            return new TimestampRepository();
        }
    }
}