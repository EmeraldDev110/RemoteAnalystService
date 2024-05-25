
namespace RemoteAnalyst.Repository.Models
{
    public class DatabaseMapping
    {
        public DatabaseMapping() { }
        public virtual string SystemSerial { get; set; }
        public virtual string ConnectionString { get; set; }
        public virtual System System { get; set; }
    }
}
