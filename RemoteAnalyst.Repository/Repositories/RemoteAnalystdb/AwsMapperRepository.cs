using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;

namespace RemoteAnalyst.Repository.Repositories
{
    public class AwsMapperRepository
    {
        public AwsMapper GetLoaderInfo(string ec2Name)
        {
            using (ISession session = NHibernateHelper.OpenSession("AwsMapper"))
            {
                AwsMapper awsMapper = session
                .CreateCriteria(typeof(AwsMapper))
                    .Add(Restrictions.Eq("AwsName", ec2Name))
                    .UniqueResult<AwsMapper>();
                //return CollectionHelper.ToDataTable(awsMapper);
                return awsMapper;
            }
        }

        public int GetMaxLoaderSequenceNum()
        {
            using (ISession session = NHibernateHelper.OpenSession("AwsMapper"))
            {
                int res = session.QueryOver<AwsMapper>()
                    .Where(a => a.IsLoader)
                    .Select(Projections.Max<AwsMapper>(a => a.SequenceNumber))
                    .SingleOrDefault<int>();
                return res;
            }
        }

        public void InsertNewLoader(string ec2Name, int sequenceNum)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSession("AwsMapper"))
                using (ITransaction transaction = session.BeginTransaction())
                {
                    AwsMapper awsMapper = new AwsMapper(ec2Name, sequenceNum);
                    session.Save(awsMapper);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteLoader(string ec2Name)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSession("AwsMapper"))
                {
                    session.Query<AwsMapper>()
                        .Where(a => a.AwsName == ec2Name)
                        .Delete();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
