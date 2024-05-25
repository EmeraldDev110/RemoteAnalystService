using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.BaseClass {
    interface IHeaderInfo {
        void ReadHeader(string uwsPath, ILog log, Header header);

        Indices ReaderEntityHeader(string uwsPath, ILog log, int indexPosition, long dataPosition);
    }
}
