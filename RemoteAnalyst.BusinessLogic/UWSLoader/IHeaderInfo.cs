using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;

namespace RemoteAnalyst.BusinessLogic.UWSLoader {
    public interface IHeaderInfo {
        void ReadHeader(string uwsPath, ILog log, Header header);

        Indices ReaderEntityHeader(string uwsPath, ILog log, int indexPosition, long dataPosition);
    }
}
