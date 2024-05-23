using System;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class TransactionProfileTrendServices {
        private readonly string _connectionString;

        public TransactionProfileTrendServices(string connectionString) {
            _connectionString = connectionString;
        }

        public bool CheckTransactionProfileTrendFor(string dbName) {
            var transactionProfileTrend = new TransactionProfileTrends(_connectionString);
            var exists = transactionProfileTrend.CheckTransactionProfileTrends(dbName);

            return exists;
        }

        public void CreateTransactionProfileTrendFor() {
            var transactionProfileTrend = new TransactionProfileTrends(_connectionString);
            transactionProfileTrend.CreateTransactionProfileTrends();
        }

        public void InsertNewDataFor(int profileId, DateTime fromDateTime, DateTime toDateTime, double tps) {
            var transactionProfileTrend = new TransactionProfileTrends(_connectionString);

            var exists = transactionProfileTrend.CheckDuplicatedTrend(profileId, fromDateTime, toDateTime);
            if(!exists)
                transactionProfileTrend.InsertNewData(profileId, fromDateTime, toDateTime, tps);
            else
                transactionProfileTrend.UpdateTrendData(profileId, fromDateTime, toDateTime, tps);
        }

        public void PopulateAnyTPSFor(int profileId, string fileTableName, DateTime fromDateTime, DateTime toDateTime, long interval,
                                      double transactionRatio, short transactionCounter, string volume, string subVol, string fileName, 
                                      bool isCpuToFile, long cpuInterval, string tempSaveLocation) {
            var fileEntity = new FileEntityRepository(_connectionString, transactionCounter, volume, subVol, fileName);

            if (!cpuInterval.Equals(interval)) {
                if (isCpuToFile) {
                    for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                        var trendData = fileEntity.GetAnyTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, interval);
                        InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);

                        var cpuTableName = fileTableName.Replace("FILE", "CPU");

						//new logic 
						var trendCPUNormalizedName = "TrendCPUNormalized";
                        var databaseName = Helper.FindKeyName(_connectionString, "DATABASE");
                        var database = new Database(_connectionString);
                        var exists = database.CheckTableExists(trendCPUNormalizedName, databaseName);
						TrendCPUNormalized trendCPUNormalizedTable = new TrendCPUNormalized();

                        if (!exists) {
							trendCPUNormalizedTable.CreateTrendCPUNormalizedTable(_connectionString, trendCPUNormalizedName);
                            //cpuEntityTable.CreateNormalizeTable(newCpuTableName);
                        }

						//Check duplicate data.
						var dataExists = trendCPUNormalizedTable.CheckDuplicateDataFromNorTable(trendCPUNormalizedName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), _connectionString);
						//var dataExists = cpuEntityTable.CheckDuplicateDataFromNorTable(newCpuTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2));

						if (!dataExists) {
                            //Get CPU DATA, and normalize the data.
                            var cpuBaseData = trendCPUNormalizedTable.GetCPUBaseData(cpuTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), currentTime, currentTime.AddSeconds(interval), _connectionString);

                            if (cpuBaseData.Rows.Count > 0) {
                                var dataTables = new DataTables(_connectionString);
                                dataTables.InsertEntityData(trendCPUNormalizedName, cpuBaseData, tempSaveLocation);
                            }
                        }
                    }
                }
                else {
                    for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                        var trendData = fileEntity.GetAnyTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, interval);
                        var newTrendData = (trendData*cpuInterval)/interval;
                        for (var newCurrentTime = currentTime; newCurrentTime < currentTime.AddSeconds(interval); newCurrentTime = newCurrentTime.AddSeconds(cpuInterval)) {
                            InsertNewDataFor(profileId, newCurrentTime, newCurrentTime.AddSeconds(cpuInterval), newTrendData);
                        }
                    }
                }
            }
            else {
                for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                    var trendData = fileEntity.GetAnyTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, interval);
                    InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);
                }
            }
        }

        public void PopulateOpenerProgramFor(int profileId, string fileTableName, DateTime fromDateTime, DateTime toDateTime, long interval,
                                      double transactionRatio, short transactionCounter, string volume, string subVol, string fileName,
                                      string openerVolume, string openerSubVol, string openerFileName, bool isCpuToFile, long cpuInterval, string tempSaveLocation) {
            var fileEntity = new FileEntityRepository(_connectionString, transactionCounter, volume, subVol, fileName);

            /*for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                var trendData = fileEntity.GetOpenerProgramTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2),
                                                               transactionRatio, openerVolume, openerSubVol, openerFileName);
                InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);
            }*/

            if (!cpuInterval.Equals(interval)) {
                if (isCpuToFile) {
                    var cpuTableName = fileTableName.Replace("FILE", "CPU");

					//new logic refers to trendCPUNormalized table from ..._Nor table
                    //var newCpuTableName = cpuTableName + "_Nor";
					var trendCPUNormalizedName = "TrendCPUNormalized";
					var databaseName = Helper.FindKeyName(_connectionString, "DATABASE");
                    var database = new Database(_connectionString);
                    //var exists = database.CheckTableExists(newCpuTableName, databaseName);
					var exists = database.CheckTableExists(trendCPUNormalizedName, databaseName);
					//var cpuEntityTable = new CPUEntityTable(_connectionString);
					TrendCPUNormalized trendCPUNormalizedTable = new TrendCPUNormalized();

					if (!exists) {
                        //cpuEntityTable.CreateNormalizeTable(newCpuTableName);
						trendCPUNormalizedTable.CreateTrendCPUNormalizedTable(_connectionString, trendCPUNormalizedName);
					}

                    for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                        var trendData = fileEntity.GetOpenerProgramTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2),
                                                               transactionRatio, openerVolume, openerSubVol, openerFileName, interval);
                        InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);
                        
                        //Check duplicate data.
                        //var dataExists = cpuEntityTable.CheckDuplicateDataFromNorTable(newCpuTableName, currentTime, currentTime.AddSeconds(interval));
						var dataExists = trendCPUNormalizedTable.CheckDuplicateDataFromNorTable(trendCPUNormalizedName, currentTime, currentTime.AddSeconds(interval), _connectionString);
						if (!dataExists) {
                            //Get CPU DATA, and normalize the data.
                            //var cpuBaseData = cpuEntityTable.GetCPUBaseData(cpuTableName, currentTime, currentTime.AddSeconds(interval), currentTime, currentTime.AddSeconds(interval));
							var cpuBaseData = trendCPUNormalizedTable.GetCPUBaseData(cpuTableName, currentTime, currentTime.AddSeconds(interval), currentTime, currentTime.AddSeconds(interval), _connectionString);
							if (cpuBaseData.Rows.Count > 0) {
                                var dataTables = new DataTables(_connectionString);
                                //dataTables.InsertEntityData(newCpuTableName, cpuBaseData, tempSaveLocation);
								dataTables.InsertEntityData(trendCPUNormalizedName, cpuBaseData, tempSaveLocation);
							}
                        }
                    }
                }
                else {
                    for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                        var trendData = fileEntity.GetOpenerProgramTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2),
                                                               transactionRatio, openerVolume, openerSubVol, openerFileName, interval);
                        var newTrendData = (trendData * cpuInterval) / interval;

                        for (var newCurrentTime = currentTime; newCurrentTime < currentTime.AddSeconds(interval); newCurrentTime = newCurrentTime.AddSeconds(cpuInterval)) {
                            InsertNewDataFor(profileId, newCurrentTime, newCurrentTime.AddSeconds(cpuInterval), newTrendData);
                        }
                    }
                }
            }
            else {
                for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                    var trendData = fileEntity.GetOpenerProgramTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2),
                                                               transactionRatio, openerVolume, openerSubVol, openerFileName, interval);
                    InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);
                }
            }
        }

        public void PopulateOpenerProcessFor(int profileId, string fileTableName, DateTime fromDateTime, DateTime toDateTime, long interval,
                                      double transactionRatio, short transactionCounter, string volume, string subVol, string fileName, 
                                      string openerProcess, bool isCpuToFile, long cpuInterval, string tempSaveLocation) {
            var fileEntity = new FileEntityRepository(_connectionString, transactionCounter, volume, subVol, fileName);

            /*for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                var trendData = fileEntity.GetOpenerProcessTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, openerProcess);

                InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);
            }*/
            if (!cpuInterval.Equals(interval)) {
                if (isCpuToFile) {
                    for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                        var trendData = fileEntity.GetOpenerProcessTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, openerProcess, interval);
                        InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);

                        var cpuTableName = fileTableName.Replace("FILE", "CPU");
                        //var newCpuTableName = cpuTableName + "_Nor";
						var trendCPUNormalizedName = "TrendCPUNormalized";
						var databaseName = Helper.FindKeyName(_connectionString, "DATABASE");
                        var database = new Database(_connectionString);
                        //var exists = database.CheckTableExists(newCpuTableName, databaseName);
						var exists = database.CheckTableExists(trendCPUNormalizedName, databaseName);
						//var cpuEntityTable = new CPUEntityTable(_connectionString);
						TrendCPUNormalized trendCPUNormalizedTable = new TrendCPUNormalized();

						if (!exists) {
                            //cpuEntityTable.CreateNormalizeTable(newCpuTableName);
							trendCPUNormalizedTable.CreateTrendCPUNormalizedTable(_connectionString, trendCPUNormalizedName);
						}

                        //Check duplicate data.
                        //var dataExists = cpuEntityTable.CheckDuplicateDataFromNorTable(newCpuTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2));
						var dataExists = trendCPUNormalizedTable.CheckDuplicateDataFromNorTable(trendCPUNormalizedName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), _connectionString);
						if (!dataExists) {
                            //Get CPU DATA, and normalize the data.
                            //var cpuBaseData = cpuEntityTable.GetCPUBaseData(cpuTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), currentTime, currentTime.AddSeconds(interval));
							var cpuBaseData = trendCPUNormalizedTable.GetCPUBaseData(cpuTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), currentTime, currentTime.AddSeconds(interval), _connectionString);
							if (cpuBaseData.Rows.Count > 0) {
                                var dataTables = new DataTables(_connectionString);
                                //dataTables.InsertEntityData(newCpuTableName, cpuBaseData, tempSaveLocation);
								dataTables.InsertEntityData(trendCPUNormalizedName, cpuBaseData, tempSaveLocation);
							}
                        }
                    }
                }
                else {
                    for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                        var trendData = fileEntity.GetOpenerProcessTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, openerProcess, interval);
                        var newTrendData = (trendData * cpuInterval) / interval;
                        for (var newCurrentTime = currentTime; newCurrentTime < currentTime.AddSeconds(interval); newCurrentTime = newCurrentTime.AddSeconds(cpuInterval)) {
                            InsertNewDataFor(profileId, newCurrentTime, newCurrentTime.AddSeconds(cpuInterval), newTrendData);
                        }
                    }
                }
            }
            else {
                for (var currentTime = fromDateTime; currentTime < toDateTime; currentTime = currentTime.AddSeconds(interval)) {
                    var trendData = fileEntity.GetOpenerProcessTPS(fileTableName, currentTime.AddSeconds(interval * -0.2), currentTime.AddSeconds(interval).AddSeconds(interval * 0.2), transactionRatio, openerProcess, interval);
                    InsertNewDataFor(profileId, currentTime, currentTime.AddSeconds(interval), trendData);
                }
            }
        }
    }
}
