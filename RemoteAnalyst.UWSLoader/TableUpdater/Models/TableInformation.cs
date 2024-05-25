using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.TableUpdater.Models {
    public class TableInformation {
        public string TableName { get; set; }
        public List<string> RowUpdates { get; set; }
        public List<string> RowUpdateInfo { get; set; }
        public List<string> RowAdds { get; set; }
        public string PrimaryKey { get; set; }
    }
}
