using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class AwsMapper
    {
        public AwsMapper(string awsName, int sequenceNumber)
        {
            AwsName = awsName;
            IsLoader = true;
            SequenceNumber = sequenceNumber;
            IsProduction = false;
            IsAurora = false;
            OldRDS = false;
        }
        public AwsMapper()
        {
        }

        public virtual int MapperId { get; set; }
        public virtual string AwsName { get; set; }
        public virtual bool IsLoader { get; set; }
        public virtual int SequenceNumber { get; set; }
        public virtual bool IsProduction { get; set; }
        public virtual bool IsAurora { get; set; }
        public virtual bool OldRDS { get; set; }
        public virtual bool ProdType { get; set; }
    }
}
