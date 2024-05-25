using RemoteAnalyst.TransMon.TransMonFactoryPattern.Controller;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Repository;

namespace RemoteAnalyst.TransMon.FactoryPatternExample.Controller {
    internal abstract class AbstractFactory {
        public abstract AbstractService CreateServiceObj(IRepository iRepository);
        public abstract AbstractRepository CreateRepositoryObj();
    }
}