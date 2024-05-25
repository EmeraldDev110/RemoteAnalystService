using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using MySqlConnector;
using NHibernate.Transform;

namespace RemoteAnalyst.Repository.Repositories
{
    public class SystemWeekThresholdsRepository
    {
        string _connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
        public DataTable GetCpuBusy(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT CPUBusyMinor, CpuBusyMajor FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.CPUBusyMinor)
                        .Select(x => x.CPUBusyMajor)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "CPUBusyMinor", "CPUBusyMajor" } );
            }

            return data;
        }

        public DataTable GetCpuQueueLength(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT CPUQueueLengthMinor, CPUQueueLengthMajor FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.CPUQueueLengthMinor)
                        .Select(x => x.CPUQueueLengthMajor)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "CPUQueueLengthMinor", "CPUQueueLengthMajor" });
            }

            return data;
        }

        public DataTable GetIpuBusy(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT IPUBusyMinor, IPUBusyMajor FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.IPUBusyMinor)
                        .Select(x => x.IPUBusyMajor)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "IPUBusyMinor", "IPUBusyMajor" });
            }

            return data;
        }

        public DataTable GetIpuQueueLength(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT IPUQueueLengthMinor, IPUQueueLengthMajor FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.IPUQueueLengthMinor)
                        .Select(x => x.IPUQueueLengthMajor)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "IPUQueueLengthMinor", "IPUQueueLengthMajor" });
            }
            return data;
        }

        public DataTable GetDiskQueueLength(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT DiskQueueLengthMinor, DiskQueueLengthMajor FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.DiskQueueLengthMinor)
                        .Select(x => x.DiskQueueLengthMajor)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "DiskQueueLengthMinor", "DiskQueueLengthMajor" });
            }
            return data;
        }

        public DataTable GetDiskDP2(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT DiskDP2Minor, DiskDP2Major FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.DiskDP2Minor)
                        .Select(x => x.DiskDP2Major)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "DiskDP2Minor", "DiskDP2Major" });
            }
            return data;
        }

        public DataTable GetStorage(string systemSerial, int thresholdTypeId)
        {
            var cmdText = @"SELECT StorageMinor, StorageMajor FROM SystemWeekThresholds WHERE SystemSerial = @SystemSerial AND ThresholdTypeId = @ThresholdTypeId";

            var data = new DataTable();
            using (ISession session = NHibernateHelper.OpenSession())
            {
                ICollection<SystemWeekThresholds> res = session.QueryOver<SystemWeekThresholds>()
                    .Where(x => x.SystemSerial == systemSerial)
                    .And(x => x.ThresholdTypeId == thresholdTypeId)
                    .SelectList(list => list
                        .Select(x => x.StorageMinor)
                        .Select(x => x.StorageMajor)
                    )
                    .TransformUsing(Transformers.AliasToBean<SystemWeekThresholds>())
                    .List();
                data = CollectionHelper.ToDataTable(res, new List<string> { "StorageMinor", "StorageMajor" });
            }

            return data;
        }
    }
}