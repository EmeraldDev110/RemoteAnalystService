using System.Data;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Controller;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Repository;

namespace RemoteAnalyst.TransMon.TransMonFactoryPattern.Service {
    public class TimestampService : IService, AbstractService {
        public TimestampService(IRepository iRepository) : base(iRepository) {
        }

        public DataTable GetWithParam(string param) {
            return Get();
        }
    }
}