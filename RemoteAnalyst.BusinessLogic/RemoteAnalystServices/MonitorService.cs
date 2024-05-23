using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.AWS.CloudWatch;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.RDS;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class MonitorService {
        private readonly string _connectionString;
        private readonly ILog _log;

        public MonitorService(string connectionString, ILog log) {
            _connectionString = connectionString;
            _log = log;
        }

		public double GetRDSCpuBusy(string rdsName) {
			var monitorRDS = new MonitorRDS(_connectionString);
			return monitorRDS.GetRDSCpuBusy(rdsName);
		}


		public void LoadMonitorData() {
			var ec2Data = GetEc2();
			if (ec2Data.Count > 0) {
				var monitorEC2 = new MonitorEC2(_connectionString);
				monitorEC2.DeleteAllEntry();

				foreach (var ec2 in ec2Data) {
					var exists = monitorEC2.CheckDataEntry(ec2.InstanceId);

					if (exists) {
						monitorEC2.UpdateEntry(ec2.InstanceId, ec2.EC2Name, ec2.InstanceName, ec2.CpuBusy, ec2.TodayLoadCount, ec2.TodayLoadSize, ec2.CpuBusyAverage, ec2.CpuBusyPeak);
					}
					else {
						monitorEC2.InsertEntry(ec2.InstanceId, ec2.EC2Name, ec2.InstanceName, ec2.CpuBusy, ec2.TodayLoadCount, ec2.TodayLoadSize, ec2.CpuBusyAverage, ec2.CpuBusyPeak);
					}
				}
			}

			var rdsData = GetRds();
            if (rdsData.Count > 0) {
                var monitorRDS = new MonitorRDS(_connectionString);

                foreach (var rds in rdsData) {
                    var exists = monitorRDS.CheckDataEntry(rds.RdsName);

                    if (exists) {
						if (rds.CpuBusy != 0d) {
							monitorRDS.UpdateEntry(rds.RdsName, rds.RdsRealName, rds.CpuBusy, rds.GbSize, rds.FreeSpace, rds.TodayLoadCount, rds.TodayLoadSize, rds.CpuBusyAverage, rds.CpuBusyPeak, rds.DisplaySpace);
						}
						else {
							monitorRDS.UpdateEntryNoCpuBusy(rds.RdsName, rds.RdsRealName, rds.GbSize, rds.FreeSpace, rds.TodayLoadCount, rds.TodayLoadSize, rds.CpuBusyAverage, rds.CpuBusyPeak, rds.DisplaySpace);
						}
                    }
                    else {
                        monitorRDS.InsertEntry(rds.RdsName, rds.RdsRealName, rds.CpuBusy, rds.GbSize, rds.FreeSpace, rds.TodayLoadCount, rds.TodayLoadSize, rds.CpuBusyAverage, rds.CpuBusyPeak, rds.DisplaySpace);
                    }
                }
            }
        }


        public List<EC2View> GetEc2() {
            var ec2 = new AmazonEC2();
            var ec2Infos = ec2.GetLoaderEc2Information();
            var ec2List = new List<EC2View>();
            var loadingInfoService = new LoadingInfoService(_connectionString);
            var cloudWatch = new AmazonCloudWatch();
            var awsMapper = new AwsMapperService(_connectionString);

            foreach (var ec2Info in ec2Infos) {
                var loadInfo = loadingInfoService.GetLoadCountTodayFor(ec2Info.InstanceId);
                var loaderName = awsMapper.BuildLoaderNameFor(ec2Info.InstanceName, ec2Info.InstanceType);

                //Format the name and get LT, GBT, %AT, %PT
                ec2List.Add(new EC2View {
                    InstanceId = ec2Info.InstanceId,
                    EC2Name = ec2Info.InstanceName,
                    InstanceName = loaderName,
                    CpuBusy = cloudWatch.GetEC2CpuBusy(ec2Info.InstanceId),
                    CpuBusyAverage = Math.Round(cloudWatch.GetEC2CpuBusyAverage(ec2Info.InstanceId), 2),
                    CpuBusyPeak = Math.Round(cloudWatch.GetEC2CpuBusyPeak(ec2Info.InstanceId), 2),
                    TodayLoadCount = loadInfo.Count,
                    TodayLoadSize = Math.Round(loadInfo.Sum(x => x) / Convert.ToDouble(1024 * 1024 * 1024), 2)
                });
            }
            return ec2List.OrderBy(x => x.InstanceName).ToList();
        }

        public List<RdsView> GetRds() {
            var rds = new AmazonRDS();
            var rdsNames = rds.GetRDSInstances();
            var rdsList = new List<RdsView>();
            var cloudWatch = new AmazonCloudWatch();
            var loadingInfoService = new LoadingInfoService(_connectionString);
            var awsMapper = new AwsMapperService(_connectionString);
            var databaseMappings = new DatabaseMappingService(_connectionString);

            foreach (var rdsInfo in rdsNames) {
                try {
                    var loadInfoMeasure = loadingInfoService.GetRdsLoadCountTodayFor(rdsInfo.RdsName);
                    var loadInfoOther = loadingInfoService.GetRdsOtherLoadCountTodayFor(rdsInfo.RdsName);
                    var rdsName = awsMapper.BuildRdsNameFor(rdsInfo.RdsName, rdsInfo.RdsType);

                    var gbSize = 0;
                    var freeSpace = 0d;
                    var displaySpace = "";

                    if (!rdsInfo.IsAurora) {
                        gbSize = rds.GetRDSAllocatedStorage(rdsInfo.RdsName);
                        freeSpace = cloudWatch.GetRDSFreeSpace(rdsInfo.RdsName);
                        displaySpace = Math.Round(freeSpace, 0).ToString("#,##0") + " / " + Math.Round((gbSize - freeSpace), 0).ToString("#,##0");
                    }
                    else {
                        var rdsConnectionString = databaseMappings.GetRdsConnectionStringFor(rdsInfo.RdsName);
                        var dataDictionary = new DataDictionary(rdsConnectionString);
						var totalSpace = cloudWatch.GetAuroraUsedSpace(rdsInfo.RdsName);
						var storageAnalystService = new StorageAnalysisService(rdsConnectionString, _log);
						var usedSpace = storageAnalystService.GetActiveStorage() / 1024;
						freeSpace = totalSpace - usedSpace;

                        if (freeSpace != 0.0)
                            displaySpace = (Math.Round(totalSpace, 0) - Math.Round(usedSpace, 0)).ToString("#,##0") + " / " + Math.Round(usedSpace, 0).ToString("#,##0");
                        else
                            displaySpace = "N/A";
                    }

					rdsList.Add(new RdsView {
						RdsName = rdsName,
						RdsRealName = rdsInfo.RdsName,
						CpuBusy = cloudWatch.GetRDSCpuBusy(rdsInfo.RdsName),
						GbSize = gbSize,
						FreeSpace = freeSpace,
						CpuBusyAverage = Math.Round(cloudWatch.GetRDSCpuBusyAverage(rdsInfo.RdsName), 2),
						CpuBusyPeak = Math.Round(cloudWatch.GetRDSCpuBusyPeak(rdsInfo.RdsName), 2),
						TodayLoadCount = loadInfoMeasure.Count.ToString("#,##0") + "|" + loadInfoOther.Count.ToString("#,##0"),
						TodayLoadSize = Math.Round(loadInfoMeasure.Sum(x => x) / Convert.ToDouble(1024 * 1024 * 1024), 2).ToString("#,##0.00") + "|" + Math.Round(loadInfoOther.Sum(x => x) / Convert.ToDouble(1024 * 1024 * 1024), 2).ToString("#,##0.00"),
						//AssignedSystem = rdsSystems,
						DisplaySpace = displaySpace
					});
				}
                catch (Exception ex) {
                    //Skip the display.
                }
            }

            return rdsList.OrderBy(x => x.RdsName).ToList();
        }

        public DataTable GetEC2LoaderIPInformation()
        {
            var monitorEC2 = new MonitorEC2(_connectionString);
            return monitorEC2.GetEC2LoaderIPInformation();
        }

        public bool IsLoaderActive(string instanceId)
        {
            var monitorEC2 = new MonitorEC2(_connectionString);
            return monitorEC2.CheckIsActive(instanceId);
        }
    }
}
