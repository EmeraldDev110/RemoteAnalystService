using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;

namespace RemoteAnalyst.BusinessLogic.UWSLoader {
    public class HeaderInfoV1 : IHeaderInfo {
        void IHeaderInfo.ReadHeader(string uwsPath, ILog log, Header header) {
            log.Info("***************************UWS Header***************************");

            using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //using (StreamReader reader = new StreamReader(stream))
                using (var reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();
                    int byteLocation = 0;

                    //H-File-Creation-Timestamp (20)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewUwsFileCreationTimeStamp = reader.ReadBytes(header.NewUwsFileCreationTimeStamp.Length);
                    header.UwsUwsFileCreationTimeStamp = Helper.RemoveNULL(myEncoding.GetString(header.NewUwsFileCreationTimeStamp).Trim());
                    byteLocation += 20;
                    log.InfoFormat("UwsUwsFileCreationTimeStamp: {0}", header.UwsUwsFileCreationTimeStamp.Trim());
                    

                    //H-System-Node-Name (8)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewUwsSystemNameByte = reader.ReadBytes(header.NewUwsSystemNameByte.Length);
                    header.UwsSystemName = Helper.RemoveNULL(myEncoding.GetString(header.NewUwsSystemNameByte).Trim());
                    byteLocation += 8;
                    log.InfoFormat("UwsSystemName: {0}", header.UwsSystemName.Trim());
                    

                    //H-System-Serial-Number (20)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewSystemSerialByte = reader.ReadBytes(header.NewSystemSerialByte.Length);
                    header.UWSSerialNumber = Helper.RemoveNULL(myEncoding.GetString(header.NewSystemSerialByte).Trim());
                    while (header.UWSSerialNumber.Length < 6) {
                        header.UWSSerialNumber = "0" + header.UWSSerialNumber;
                    }
                    byteLocation += 20;
                    log.InfoFormat("UWSSerialNumber: {0}", header.UWSSerialNumber.Trim());
                    

                    //H-Creator-Name (36)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewCreatorName = reader.ReadBytes(header.NewCreatorName.Length);
                    header.UwsCreatorName = Helper.RemoveNULL(myEncoding.GetString(header.NewCreatorName).Trim());
                    byteLocation += 36;
                    log.InfoFormat("UwsCreatorName: {0}", header.UwsCreatorName.Trim());
                    

                    //H-Creator-Vproc (50)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewCreatorVproc = reader.ReadBytes(header.NewCreatorVproc.Length);
                    header.UwsCreatorVproc = Helper.RemoveNULL(myEncoding.GetString(header.NewCreatorVproc).Trim());
                    byteLocation += 50;
                    log.InfoFormat("UwsCreatorVproc: {0}", header.UwsCreatorVproc.Trim());
                    

                    //H-Measure-DLL-Version (4)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewMeasureDllVersion = reader.ReadBytes(header.NewMeasureDllVersion.Length);
                    header.UwsMeasureDllVersion = Helper.RemoveNULL(myEncoding.GetString(header.NewMeasureDllVersion).Trim());
                    byteLocation += 4;
                    log.InfoFormat("UwsMeasureDllVersion: {0}", header.UwsMeasureDllVersion.Trim());
                    

                    //H-UWS-DLL-Version (4)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewUwsDllVersion = reader.ReadBytes(header.NewUwsDllVersion.Length);
                    header.UwsUwsDllVersion = Helper.RemoveNULL(myEncoding.GetString(header.NewUwsDllVersion).Trim());
                    byteLocation += 4;
                    log.InfoFormat("UwsUwsDllVersion: {0}", header.UwsUwsDllVersion.Trim());
                    

                    //H-Measure-File-Location (36)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewMeasureFileLocation = reader.ReadBytes(header.NewMeasureFileLocation.Length);
                    header.UwsMeasureFileLocation = Helper.RemoveNULL(myEncoding.GetString(header.NewMeasureFileLocation).Trim());
                    byteLocation += 36;
                    log.InfoFormat("UwsMeasureFileLocation: {0}", header.UwsMeasureFileLocation.Trim());
                    

                    //H-Measure-File-Size (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewMeasureFileSize = reader.ReadBytes(header.NewMeasureFileSize.Length);
                    header.UwsMeasureFileSize = Helper.RemoveNULL(myEncoding.GetString(header.NewMeasureFileSize).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsMeasureFileSize: {0}", header.UwsMeasureFileSize.Trim());
                    

                    //H-Measure-File-Count (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewMeasureFileCount = reader.ReadBytes(header.NewMeasureFileCount.Length);
                    header.UwsMeasureFileCount = Helper.RemoveNULL(myEncoding.GetString(header.NewMeasureFileCount).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsMeasureFileCount: {0}", header.UwsMeasureFileCount.Trim());
                    

                    //H-UWS-File-Location (36)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewUwsFileLocation = reader.ReadBytes(header.NewUwsFileLocation.Length);
                    header.UwsUwsFileLocation = Helper.RemoveNULL(myEncoding.GetString(header.NewUwsFileLocation).Trim());
                    byteLocation += 36;
                    log.InfoFormat("UwsUwsFileLocation: {0}", header.UwsUwsFileLocation.Trim());
                    

                    //H-Header-Size (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewHeaderSize = reader.ReadBytes(header.NewHeaderSize.Length);
                    header.UwsHeaderSize = Helper.RemoveNULL(myEncoding.GetString(header.NewHeaderSize).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsHeaderSize: {0}", header.UwsHeaderSize.Trim());
                    

                    //H-Entity-Header-Length (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewEntityHeaderLength = reader.ReadBytes(header.NewEntityHeaderLength.Length);
                    header.UwsEntityHeaderLength = Helper.RemoveNULL(myEncoding.GetString(header.NewEntityHeaderLength).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsEntityHeaderLength: {0}", header.UwsEntityHeaderLength.Trim());
                    

                    //H-Coll-Info-Start-Timestamp (20)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewCollInfoStartTimestamp = reader.ReadBytes(header.NewCollInfoStartTimestamp.Length);
                    header.UwsCollInfoStartTimestamp = Helper.RemoveNULL(myEncoding.GetString(header.NewCollInfoStartTimestamp).Trim());
                    byteLocation += 20;
                    log.InfoFormat("UwsCollInfoStartTimestamp: {0}", header.UwsCollInfoStartTimestamp.Trim());
                    

                    //H-Coll-Info-End-Timestamp (20)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewCollInfoEndTimestamp = reader.ReadBytes(header.NewCollInfoEndTimestamp.Length);
                    header.UwsCollInfoEndTimestamp = Helper.RemoveNULL(myEncoding.GetString(header.NewCollInfoEndTimestamp).Trim());
                    byteLocation += 20;
                    log.InfoFormat("UwsCollInfoEndTimestamp: {0}", header.UwsCollInfoEndTimestamp.Trim());
                    

                    //H-Coll-Info-Interval (6)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewCollInfoInterval = reader.ReadBytes(header.NewCollInfoInterval.Length);
                    header.UwsCollInfoInterval = Helper.RemoveNULL(myEncoding.GetString(header.NewCollInfoInterval).Trim());
                    byteLocation += 6;
                    log.InfoFormat("UwsCollInfoInterval: {0}", header.UwsCollInfoInterval.Trim());
                    

                    //H-UWS-File-Size (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewUwsFileSize = reader.ReadBytes(header.NewUwsFileSize.Length);
                    header.UwsUwsFileSize = Helper.RemoveNULL(myEncoding.GetString(header.NewUwsFileSize).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsUwsFileSize: {0}", header.UwsUwsFileSize.Trim());
                    

                    //H-Entity-Header-Total-Count (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewEntityHeaderTotalCount = reader.ReadBytes(header.NewEntityHeaderTotalCount.Length);
                    header.UwsEntityHeaderTotalCount = Helper.RemoveNULL(myEncoding.GetString(header.NewEntityHeaderTotalCount).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsEntityHeaderTotalCount: {0}", header.UwsEntityHeaderTotalCount.Trim());
                    

                    //H-Entity-Unique-Type-Count (10)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewEntityUniqueTypeCount = reader.ReadBytes(header.NewEntityUniqueTypeCount.Length);
                    header.UwsEntityUniqueTypeCount = Helper.RemoveNULL(myEncoding.GetString(header.NewEntityUniqueTypeCount).Trim());
                    byteLocation += 10;
                    log.InfoFormat("UwsEntityUniqueTypeCount: {0}", header.UwsEntityUniqueTypeCount.Trim());
                    

                    //H-Proc-Info-Start-Timestamp (20)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewProcInfoStartTimestamp = reader.ReadBytes(header.NewProcInfoStartTimestamp.Length);
                    header.UwsProcInfoStartTimestamp = Helper.RemoveNULL(myEncoding.GetString(header.NewProcInfoStartTimestamp).Trim());
                    byteLocation += 20;
                    log.InfoFormat("UwsProcInfoStartTimestamp: {0}", header.UwsProcInfoStartTimestamp.Trim());
                    

                    //H-Proc-Info-End-Timestamp (20)
                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                    header.NewProcInfoEndTimestamp = reader.ReadBytes(header.NewProcInfoEndTimestamp.Length);
                    header.UwsProcInfoEndTimestamp = Helper.RemoveNULL(myEncoding.GetString(header.NewProcInfoEndTimestamp).Trim());
                    log.InfoFormat("UwsProcInfoEndTimestamp: {0}", header.UwsProcInfoEndTimestamp.Trim());
                    

                    header.UwsHLen = Convert.ToInt16(header.UwsHeaderSize);
                    header.UwsXRecords = Convert.ToInt16(header.UwsEntityHeaderTotalCount);
                    header.UwsXLen = Convert.ToInt16(header.UwsEntityHeaderLength);
                }
            }
        }


        public Indices ReaderEntityHeader(string uwsPath, ILog log, int indexPosition, long dataPosition) {
            var indexer = new Indices();

            using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //using (StreamReader reader = new StreamReader(stream))
                using (var reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();

                    #region Index

                    log.InfoFormat("***************************Entity Header***************************");
                    
                    int byteCounter = 0;
                    reader.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
                    //indexBytes = reader.ReadBytes(indexBytes.Length);
                    byte[] newIndexBytes = reader.ReadBytes(126); //126 is size of each entity header.

                    //Get Index Name (first 8 bytes).
                    indexer.FName = myEncoding.GetString(newIndexBytes, 0, 7);
                    byteCounter += 8;
                    log.InfoFormat("indexer.FName: {0}", indexer.FName);
                    

                    //Index Type.
                    indexer.FType = Convert.ToInt16(myEncoding.GetString(newIndexBytes, byteCounter, 9));
                    byteCounter += 10;
                    log.InfoFormat("indexer.FType: {0}", indexer.FType);
                    

                    //Index Length.
                    indexer.FReclen = Convert.ToInt16(myEncoding.GetString(newIndexBytes, byteCounter, 9));
                    byteCounter += 10;
                    log.InfoFormat("indexer.FReclen: {0}", indexer.FReclen);
                    

                    //Index Start Time.
                    indexer.CollEntityStartTime = Convert.ToDateTime(myEncoding.GetString(newIndexBytes, byteCounter, 19));
                    byteCounter += 20;
                    log.InfoFormat("indexer.CollEntityStartTime: {0}", indexer.CollEntityStartTime);
                    

                    //Index Stop Time
                    indexer.CollEntityStoptTime = Convert.ToDateTime(myEncoding.GetString(newIndexBytes, byteCounter, 19));
                    byteCounter += 20;
                    log.InfoFormat("indexer.CollEntityStoptTime: {0}", indexer.CollEntityStoptTime);
                    

                    //Index Interval.
                    indexer.FInterval = Convert.ToInt64(myEncoding.GetString(newIndexBytes, byteCounter, 7));
                    byteCounter += 8;
                    log.InfoFormat("indexer.FInterval: {0}", indexer.FInterval);
                    

                    //Index Dump Occurs.
                    var tempVal = 0;
                    bool canConvert = int.TryParse(myEncoding.GetString(newIndexBytes, byteCounter, 9), out tempVal);

                    if (canConvert)
                        indexer.FRecords = Convert.ToInt32(myEncoding.GetString(newIndexBytes, byteCounter, 9));
                    byteCounter += 10;
                    log.InfoFormat("indexer.FRecords: {0}", indexer.FRecords);
                    

                    //Index Start Day.
                    var tempDate = new DateTime();
                    canConvert = DateTime.TryParse(myEncoding.GetString(newIndexBytes, byteCounter, 19), out tempDate);
                    if (canConvert)
                        indexer.ProcEntityStartTime = Convert.ToDateTime(myEncoding.GetString(newIndexBytes, byteCounter, 19));
                    byteCounter += 20;
                    log.InfoFormat("indexer.ProcEntityStartTime: {0}", indexer.ProcEntityStartTime);
                    

                    //Index Stop Day.
                    canConvert = DateTime.TryParse(myEncoding.GetString(newIndexBytes, byteCounter, 19), out tempDate);
                    if (canConvert)
                        indexer.ProcEntityStoptTime = Convert.ToDateTime(myEncoding.GetString(newIndexBytes, byteCounter, 19));
                    log.InfoFormat("indexer.ProcEntityStoptTime: {0}", indexer.ProcEntityStoptTime);
                    

                    //Index File Postion.
                    indexer.FilePosition = dataPosition;
                    log.InfoFormat("indexer.FilePosition: {0}", indexer.FilePosition);
                    

                    #endregion
                }
            }

            return indexer;
        }
    }
}
