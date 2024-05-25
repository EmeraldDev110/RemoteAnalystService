using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.TableUpdate.Models {
    class TableUpdateInfo {
        public string TableName { get; set; }
        public List<string> RowUpdate { get; set; }
        public string PrimaryKeys { get; set; }
    }
}
