using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;

namespace RemoteAnalyst.Repository.Repositories
{
    public class AlertRecipientRepository
    {
        public List<string> GetEmails(int processWatchId)
        {
            var emails = new List<string>();
            using (ISession session = NHibernateHelper.OpenSession("AlertRecipent"))
            {
                var res = session
                    .CreateCriteria(typeof(AlertRecipient))
                    .Add(Restrictions.Eq("Id", processWatchId))
                    .List<AlertRecipient>();
                foreach (AlertRecipient alertRecipient in res)
                {
                    if (!emails.Contains(alertRecipient.Email))
                        emails.Add(alertRecipient.Email);
                }
            }

            return emails;
        }
    }
}
