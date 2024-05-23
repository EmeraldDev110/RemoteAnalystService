using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using System.IO;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class SystemWeekExceptionService {
        private readonly string _mainConnectionString;

        public SystemWeekExceptionService(string mainConnectionString) {
            _mainConnectionString = mainConnectionString;
        }

    }
}
