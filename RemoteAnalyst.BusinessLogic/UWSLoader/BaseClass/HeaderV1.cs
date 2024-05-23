using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass {
    public class HeaderV1 {

        private byte[] _NewUwsFileCreationTimeStamp = new byte[20];
        private byte[] _NewUwsSystemNameByte = new byte[8];
        private byte[] _NewSystemSerialByte = new byte[20];
        private byte[] _NewCreatorName = new byte[36];
        private byte[] _NewCreatorVproc = new byte[50];
        private byte[] _NewMeasureDllVersion = new byte[4];
        private byte[] _NewUwsDllVersion = new byte[4];
        private byte[] _NewMeasureFileLocation = new byte[36];
        private byte[] _NewMeasureFileSize = new byte[10];
        private byte[] _NewMeasureFileCount = new byte[10];
        private byte[] _NewUwsFileLocation = new byte[36];
        private byte[] _NewHeaderSize = new byte[10];
        private byte[] _NewEntityHeaderLength = new byte[10];
        private byte[] _NewCollInfoStartTimestamp = new byte[20];
        private byte[] _NewCollInfoEndTimestamp = new byte[20];
        private byte[] _NewCollInfoInterval = new byte[6];
        private byte[] _NewUwsFileSize = new byte[10];
        private byte[] _NewEntityHeaderTotalCount = new byte[10];
        private byte[] _NewEntityUniqueTypeCount = new byte[10];
        private byte[] _NewProcInfoStartTimestamp = new byte[20];
        private byte[] _NewProcInfoEndTimestamp = new byte[20];

        public string UwsUwsFileCreationTimeStamp { get; set; }
        public string UwsCreatorName { get; set; }
        public string UwsCreatorVproc { get; set; }
        public string UwsMeasureDllVersion { get; set; }
        public string UwsUwsDllVersion { get; set; }
        public string UwsMeasureFileLocation { get; set; }
        public string UwsMeasureFileSize { get; set; }
        public string UwsMeasureFileCount { get; set; }
        public string UwsUwsFileLocation { get; set; }
        public string UwsHeaderSize { get; set; }
        public string UwsEntityHeaderLength { get; set; }
        public string UwsCollInfoStartTimestamp { get; set; }
        public string UwsCollInfoEndTimestamp { get; set; }
        public string UwsCollInfoInterval { get; set; }
        public string UwsUwsFileSize { get; set; }
        public string UwsEntityHeaderTotalCount { get; set; }
        public string UwsEntityUniqueTypeCount { get; set; }
        public string UwsProcInfoStartTimestamp { get; set; }
        public string UwsProcInfoEndTimestamp { get; set; }

        public byte[] NewUwsFileCreationTimeStamp {
            get { return _NewUwsFileCreationTimeStamp; }
            set { _NewUwsFileCreationTimeStamp = value; }
        }
        public byte[] NewUwsSystemNameByte {
            get { return _NewUwsSystemNameByte; }
            set { _NewUwsSystemNameByte = value; }
        }
        public byte[] NewSystemSerialByte {
            get { return _NewSystemSerialByte; }
            set { _NewSystemSerialByte = value; }
        }
        public byte[] NewCreatorName {
            get { return _NewCreatorName; }
            set { _NewCreatorName = value; }
        }
        public byte[] NewCreatorVproc {
            get { return _NewCreatorVproc; }
            set { _NewCreatorVproc = value; }
        }
        public byte[] NewMeasureDllVersion {
            get { return _NewMeasureDllVersion; }
            set { _NewMeasureDllVersion = value; }
        }
        public byte[] NewUwsDllVersion {
            get { return _NewUwsDllVersion; }
            set { _NewUwsDllVersion = value; }
        }
        public byte[] NewMeasureFileLocation {
            get { return _NewMeasureFileLocation; }
            set { _NewMeasureFileLocation = value; }
        }
        public byte[] NewMeasureFileSize {
            get { return _NewMeasureFileSize; }
            set { _NewMeasureFileSize = value; }
        }
        public byte[] NewMeasureFileCount {
            get { return _NewMeasureFileCount; }
            set { _NewMeasureFileCount = value; }
        }
        public byte[] NewUwsFileLocation {
            get { return _NewUwsFileLocation; }
            set { _NewUwsFileLocation = value; }
        }
        public byte[] NewHeaderSize {
            get { return _NewHeaderSize; }
            set { _NewHeaderSize = value; }
        }
        public byte[] NewEntityHeaderLength {
            get { return _NewEntityHeaderLength; }
            set { _NewEntityHeaderLength = value; }
        }
        public byte[] NewCollInfoStartTimestamp {
            get { return _NewCollInfoStartTimestamp; }
            set { _NewCollInfoStartTimestamp = value; }
        }
        public byte[] NewCollInfoEndTimestamp {
            get { return _NewCollInfoEndTimestamp; }
            set { _NewCollInfoEndTimestamp = value; }
        }
        public byte[] NewCollInfoInterval {
            get { return _NewCollInfoInterval; }
            set { _NewCollInfoInterval = value; }
        }
        public byte[] NewUwsFileSize {
            get { return _NewUwsFileSize; }
            set { _NewUwsFileSize = value; }
        }
        public byte[] NewEntityHeaderTotalCount {
            get { return _NewEntityHeaderTotalCount; }
            set { _NewEntityHeaderTotalCount = value; }
        }
        public byte[] NewEntityUniqueTypeCount {
            get { return _NewEntityUniqueTypeCount; }
            set { _NewEntityUniqueTypeCount = value; }
        }
        public byte[] NewProcInfoStartTimestamp {
            get { return _NewProcInfoStartTimestamp; }
            set { _NewProcInfoStartTimestamp = value; }
        }
        public byte[] NewProcInfoEndTimestamp {
            get { return _NewProcInfoEndTimestamp; }
            set { _NewProcInfoEndTimestamp = value; }
        }
    }
}
