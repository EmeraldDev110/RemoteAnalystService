using System;
using System.Linq;
using System.Text;
using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM {
    public class FileEntityRepository {
        private const string _mySQLTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _connectionString;
        private readonly string _counters;
        private readonly string _volume;
        private readonly string _subVol;
        private readonly string _fileName;

        public FileEntityRepository(string connectionString, short transactionCounter, string volume, string subVol, string fileName) {
            _connectionString = connectionString;

            if (transactionCounter.Equals(0)) {
                _counters = "`Reads`";
            }
            else if (transactionCounter.Equals(1)) {
                _counters = "Writes";
            }
            else {
                _counters = "`Reads` + Writes";
            }
            _volume = volume.Replace("*", "%");
            _subVol = subVol.Replace("*", "%");
            _fileName = fileName.Replace("*", "%");
        }

        public double GetAnyTPS(string fileTableName, DateTime fromDateTime, DateTime toDateTime, double transactionRatio, long fileInterval) {
            double transactions = 0;
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("FileEntity", fileTableName, _connectionString))
            {
                var res = session.CreateCriteria<FileEntity>()
                    .Add(Restrictions.Ge("FromTimestamp", fromDateTime))
                    .Add(Restrictions.Lt("FromTimestamp", toDateTime))
                    .Add(Restrictions.Gt("ToTimestamp", fromDateTime))
                    .Add(Restrictions.Le("ToTimestamp", toDateTime))
                    .Add(Restrictions.InsensitiveLike("Volume", _volume))
                    .Add(Restrictions.InsensitiveLike("SubVol", _subVol))
                    .Add(Restrictions.InsensitiveLike("FileName", _fileName))
                    .SetProjection(Projections.Sum(_counters))
                    .UniqueResult<double>();
                transactions = res / fileInterval / transactionRatio;
            }

            return transactions;
        }

        public double GetOpenerProgramTPS(string fileTableName, DateTime fromDateTime, DateTime toDateTime,
            double transactionRatio, string openerVolume, string openerSubVol, string openerFileName, long fileInterval) {
            double transactions = 0;
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("FileEntity", fileTableName, _connectionString))
            {
                var res = session.CreateCriteria<FileEntity>()
                    .Add(Restrictions.Ge("FromTimestamp", fromDateTime))
                    .Add(Restrictions.Lt("FromTimestamp", toDateTime))
                    .Add(Restrictions.Gt("ToTimestamp", fromDateTime))
                    .Add(Restrictions.Le("ToTimestamp", toDateTime))
                    .Add(Restrictions.InsensitiveLike("Volume", _volume))
                    .Add(Restrictions.InsensitiveLike("SubVol", _subVol))
                    .Add(Restrictions.InsensitiveLike("FileName", _fileName))
                    .Add(Restrictions.InsensitiveLike("OpenerVolume", openerVolume.Replace("*", "%")))
                    .Add(Restrictions.InsensitiveLike("OpenerSubVol", openerSubVol.Replace("*", "%")))
                    .Add(Restrictions.InsensitiveLike("OpenerFileName", openerFileName.Replace("*", "%")))
                    .SetProjection(Projections.Sum(_counters))
                    .UniqueResult<double>();
                transactions = res / fileInterval / transactionRatio;
            }
            return transactions;
        }

        public double GetOpenerProcessTPS(string fileTableName, DateTime fromDateTime, DateTime toDateTime, double transactionRatio, string process, long fileInterval) {
            double transactions = 0;
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("FileEntity", fileTableName, _connectionString))
            {
                var res = session.CreateCriteria<FileEntity>()
                    .Add(Restrictions.Ge("FromTimestamp", fromDateTime))
                    .Add(Restrictions.Lt("FromTimestamp", toDateTime))
                    .Add(Restrictions.Gt("ToTimestamp", fromDateTime))
                    .Add(Restrictions.Le("ToTimestamp", toDateTime))
                    .Add(Restrictions.InsensitiveLike("Volume", _volume))
                    .Add(Restrictions.InsensitiveLike("SubVol", _subVol))
                    .Add(Restrictions.InsensitiveLike("FileName", _fileName))
                    .Add(Restrictions.InsensitiveLike("OpenerProcessName", process.Replace("*", "%")))
                    .SetProjection(Projections.Sum(_counters))
                    .UniqueResult<double>();
                transactions = res / fileInterval / transactionRatio;
            }

            return transactions;
        }
    }
}