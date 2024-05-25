using System.Data;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Repository;

namespace RemoteAnalyst.TransMon.TransMonFactoryPattern.Service {
    public abstract class IService {
        private readonly IRepository _iRepository;

        protected IService(IRepository iRepository) {
            _iRepository = iRepository;
        }

        public virtual DataTable Get() {
            return _iRepository.Get();
        }

        public virtual void Delete() {
            _iRepository.Delete();
        }

        public virtual void Insert() {
            _iRepository.Insert();
        }

        public virtual void Update() {
            _iRepository.Update();
        }
    }
}