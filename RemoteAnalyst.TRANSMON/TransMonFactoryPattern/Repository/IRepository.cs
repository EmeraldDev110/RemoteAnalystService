using System.Data;

namespace RemoteAnalyst.TransMon.TransMonFactoryPattern.Repository {
    public interface IRepository {
        DataTable Get();
        void Insert();
        void Update();
        void Delete();
    }
}