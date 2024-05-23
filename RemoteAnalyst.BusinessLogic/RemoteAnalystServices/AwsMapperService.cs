using System;
using System.Collections.Generic;
using System.Linq;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class AwsMapperService {
        private readonly string _connectionString;

        public AwsMapperService(string connectionString) {
            _connectionString = connectionString;
        }
        public string BuildLoaderNameFor(string ec2Name, string ec2Type) {
            var awsMapper = new AwsMapperRepository();
            AwsMapper mapper = awsMapper.GetLoaderInfo(ec2Name);
            var loaderName = "";
            if (mapper != null) {
                var isLoader = mapper.IsLoader;
                var sequenceNumber = mapper.SequenceNumber;
                var isProduction = mapper.IsProduction;

                var tempType = ec2Type.Split('.');
                var displayType = tempType[1].ToUpper()[0];
                loaderName = (isProduction ? "P" : "S") + "L" + sequenceNumber + "-" + (displayType.ToString() == "X" ? "XL" : displayType.ToString());
            }
            else {
                loaderName = ec2Name;
            }
            return loaderName;
        }

        public string BuildRdsNameFor(string rdsName, string rdsType) {
			//AWSMapper table, column "ProdType: {RA: 0, NTS:1}" 
			var awsMapper = new AwsMapperRepository();
            AwsMapper mapper = awsMapper.GetLoaderInfo(rdsName);
            var displayName = "";
            if (mapper != null) {
                var isLoader = mapper.IsLoader;
                var sequenceNumber = mapper.SequenceNumber;
                var isProduction = mapper.IsProduction;
                var isAurora = mapper.IsAurora;
				var isOldRds = mapper.OldRDS;
				var isNTS = mapper.ProdType;

				if (isOldRds) {
					var aurora = isAurora ? "A" : "";
					var tempType = rdsType.Split('.');
					var displayType = tempType[2].ToUpper()[0];
					displayName = (isProduction ? "P" : "S") + "R" + sequenceNumber + aurora +
								  "-" + (displayType.ToString() == "X" ? "XL" : displayType.ToString());
				}
				else {
					
					var tempType = rdsType.Split('.');
					var displayType = tempType[2].ToUpper()[0];
					displayName = (isProduction ? "P" : "S") + (isNTS ? "N" : "R") + sequenceNumber.ToString("D2") +
								  "-" + (displayType.ToString() == "X" ? "XL" : displayType.ToString());
				}
            }
            else {
                displayName = rdsName;
            }

            return displayName;
        }

		public int GetMaxLoaderSequenceNum() {
			var awsMapper = new AwsMapperRepository();
			return awsMapper.GetMaxLoaderSequenceNum();
		}

		public void InsertNewLoader(string ec2Name, int sequenceNum) {
			var awsMapper = new AwsMapperRepository();
			awsMapper.InsertNewLoader(ec2Name, sequenceNum);
		}

		public void DeleteLoader(string ec2Name) {
			var awsMapper = new AwsMapperRepository();
			awsMapper.DeleteLoader(ec2Name);
		}
    }
}
