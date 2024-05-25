using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.BaseClass {
    internal class Header : HeaderV1 {
        #region Basic Header Info

        private byte[] _SystemSerialByte = new byte[10];
        private string _UWSSerialNumber = string.Empty;
        private short _UwsBinaryFormat;
        private int _UwsCPUCount;
        private int _UwsCpuMask;
        private int _UwsDataFormat;
        private string _UwsDataFormatDescription = string.Empty;
        private byte[] _UwsDataFormatDescriptionByte = new byte[64];
        private short _UwsHLen;
        private short _UwsHVersion;

        private string _UwsIdentifier = string.Empty;
        private byte[] _UwsIdentifierByte = new byte[8];
        private string _UwsKey = string.Empty;
        private byte[] _UwsKeyByte = new byte[10];
        private string _UwsOsName = string.Empty;
        private byte[] _UwsOsNameByte = new byte[64];
        private long _UwsOsVersion;
        private string _UwsOsVstring = string.Empty;
        private byte[] _UwsOsVstringByte = new byte[64];
        private string _UwsSignature = string.Empty;
        private byte[] _UwsSignatureByte = new byte[64];
        private byte[] _UwsSignatureTypeByte = new byte[18];
        private int _UwsSystemID;
        private string _UwsSystemName = string.Empty;
        private byte[] _UwsSystemNameByte = new byte[10];
        private int _UwsSystemNumber;
        private string _UwsTpdcVstring = string.Empty;
        private byte[] _UwsTpdcVstringByte = new byte[20];

        private short _UwsTpdcVstringLen;
        private int _UwsVersion;
        private string _UwsVstring = string.Empty;
        private byte[] _UwsVstringByte = new byte[30];
        private short _UwsXLen;
        private short _UwsXRecords;
        protected List<Indices> index = new List<Indices>();
        protected BinaryReader reader;

        public byte[] UwsIdentifierByte {
            get {
                return _UwsIdentifierByte;
            }
            set {
                if (_UwsIdentifierByte == value) {
                    return;
                }
                _UwsIdentifierByte = value;
            }
        }

        public byte[] UwsKeyByte {
            get {
                return _UwsKeyByte;
            }
            set {
                if (_UwsKeyByte == value) {
                    return;
                }
                _UwsKeyByte = value;
            }
        }

        public byte[] SystemSerialByte {
            get {
                return _SystemSerialByte;
            }
            set {
                if (_SystemSerialByte == value) {
                    return;
                }
                _SystemSerialByte = value;
            }
        }

        public byte[] UwsTpdcVstringByte {
            get {
                return _UwsTpdcVstringByte;
            }
            set {
                if (_UwsTpdcVstringByte == value) {
                    return;
                }
                _UwsTpdcVstringByte = value;
            }
        }

        public byte[] UwsSignatureTypeByte {
            get {
                return _UwsSignatureTypeByte;
            }
            set {
                if (_UwsSignatureTypeByte == value) {
                    return;
                }
                _UwsSignatureTypeByte = value;
            }
        }

        public byte[] UwsSignatureByte {
            get {
                return _UwsSignatureByte;
            }
            set {
                if (_UwsSignatureByte == value) {
                    return;
                }
                _UwsSignatureByte = value;
            }
        }

        public byte[] UwsDataFormatDescriptionByte {
            get {
                return _UwsDataFormatDescriptionByte;
            }
            set {
                if (_UwsDataFormatDescriptionByte == value) {
                    return;
                }
                _UwsDataFormatDescriptionByte = value;
            }
        }

        public byte[] UwsVstringByte {
            get {
                return _UwsVstringByte;
            }
            set {
                if (_UwsVstringByte == value) {
                    return;
                }
                _UwsVstringByte = value;
            }
        }

        public byte[] UwsSystemNameByte {
            get {
                return _UwsSystemNameByte;
            }
            set {
                if (_UwsSystemNameByte == value) {
                    return;
                }
                _UwsSystemNameByte = value;
            }
        }

        public byte[] UwsOsNameByte {
            get {
                return _UwsOsNameByte;
            }
            set {
                if (_UwsOsNameByte == value) {
                    return;
                }
                _UwsOsNameByte = value;
            }
        }

        public byte[] UwsOsVstringByte {
            get {
                return _UwsOsVstringByte;
            }
            set {
                if (_UwsOsVstringByte == value) {
                    return;
                }
                _UwsOsVstringByte = value;
            }
        }

        public string UwsIdentifier {
            get {
                return _UwsIdentifier;
            }
            set {
                if (_UwsIdentifier == value) {
                    return;
                }
                _UwsIdentifier = value;
            }
        }

        public string UwsKey {
            get {
                return _UwsKey;
            }
            set {
                if (_UwsKey == value) {
                    return;
                }
                _UwsKey = value;
            }
        }

        public string UWSSerialNumber {
            get {
                return _UWSSerialNumber;
            }
            set {
                if (_UWSSerialNumber == value) {
                    return;
                }
                _UWSSerialNumber = value;
            }
        }

        public string UwsTpdcVstring {
            get {
                return _UwsTpdcVstring;
            }
            set {
                if (_UwsTpdcVstring == value) {
                    return;
                }
                _UwsTpdcVstring = value;
            }
        }

        public string UwsSignature {
            get {
                return _UwsSignature;
            }
            set {
                if (_UwsSignature == value) {
                    return;
                }
                _UwsSignature = value;
            }
        }

        public string UwsDataFormatDescription {
            get {
                return _UwsDataFormatDescription;
            }
            set {
                if (_UwsDataFormatDescription == value) {
                    return;
                }
                _UwsDataFormatDescription = value;
            }
        }

        public string UwsVstring {
            get {
                return _UwsVstring;
            }
            set {
                if (_UwsVstring == value) {
                    return;
                }
                _UwsVstring = value;
            }
        }

        public string UwsSystemName {
            get {
                return _UwsSystemName;
            }
            set {
                if (_UwsSystemName == value) {
                    return;
                }
                _UwsSystemName = value;
            }
        }

        public string UwsOsName {
            get {
                return _UwsOsName;
            }
            set {
                if (_UwsOsName == value) {
                    return;
                }
                _UwsOsName = value;
            }
        }

        public string UwsOsVstring {
            get {
                return _UwsOsVstring;
            }
            set {
                if (_UwsOsVstring == value) {
                    return;
                }
                _UwsOsVstring = value;
            }
        }

        public short UwsHLen {
            get {
                return _UwsHLen;
            }
            set {
                if (_UwsHLen == value) {
                    return;
                }
                _UwsHLen = value;
            }
        }

        public short UwsHVersion {
            get {
                return _UwsHVersion;
            }
            set {
                if (_UwsHVersion == value) {
                    return;
                }
                _UwsHVersion = value;
            }
        }

        public short UwsXLen {
            get {
                return _UwsXLen;
            }
            set {
                if (_UwsXLen == value) {
                    return;
                }
                _UwsXLen = value;
            }
        }

        public short UwsXRecords {
            get {
                return _UwsXRecords;
            }
            set {
                if (_UwsXRecords == value) {
                    return;
                }
                _UwsXRecords = value;
            }
        }

        public short UwsTpdcVstringLen {
            get {
                return _UwsTpdcVstringLen;
            }
            set {
                if (_UwsTpdcVstringLen == value) {
                    return;
                }
                _UwsTpdcVstringLen = value;
            }
        }

        public short UwsBinaryFormat {
            get {
                return _UwsBinaryFormat;
            }
            set {
                if (_UwsBinaryFormat == value) {
                    return;
                }
                _UwsBinaryFormat = value;
            }
        }

        public int UwsDataFormat {
            get {
                return _UwsDataFormat;
            }
            set {
                if (_UwsDataFormat == value) {
                    return;
                }
                _UwsDataFormat = value;
            }
        }

        public int UwsVersion {
            get {
                return _UwsVersion;
            }
            set {
                if (_UwsVersion == value) {
                    return;
                }
                _UwsVersion = value;
            }
        }

        public int UwsSystemNumber {
            get {
                return _UwsSystemNumber;
            }
            set {
                if (_UwsSystemNumber == value) {
                    return;
                }
                _UwsSystemNumber = value;
            }
        }

        public int UwsSystemID {
            get {
                return _UwsSystemID;
            }
            set {
                if (_UwsSystemID == value) {
                    return;
                }
                _UwsSystemID = value;
            }
        }

        public int UwsCPUCount {
            get {
                return _UwsCPUCount;
            }
            set {
                if (_UwsCPUCount == value) {
                    return;
                }
                _UwsCPUCount = value;
            }
        }

        public int UwsCpuMask {
            get {
                return _UwsCpuMask;
            }
            set {
                if (_UwsCpuMask == value) {
                    return;
                }
                _UwsCpuMask = value;
            }
        }

        public long UwsOsVersion {
            get {
                return _UwsOsVersion;
            }
            set {
                if (_UwsOsVersion == value) {
                    return;
                }
                _UwsOsVersion = value;
            }
        }

        #endregion

        #region Header Producer Info

        private int _UwsProducer;
        private long _UwsProducerDdlVersion;
        private string _UwsProducerDdlVstring = string.Empty;
        private byte[] _UwsProducerDdlVstringByte = new byte[64];
        private string _UwsProducerName = string.Empty;
        private byte[] _UwsProducerNameByte = new byte[64];
        private long _UwsProducerVersion;
        private string _UwsProducerVstring = string.Empty;
        private byte[] _UwsProducerVstringByte = new byte[64];

        public byte[] UwsProducerNameByte {
            get {
                return _UwsProducerNameByte;
            }
            set {
                if (_UwsProducerNameByte == value) {
                    return;
                }
                _UwsProducerNameByte = value;
            }
        }

        public byte[] UwsProducerVstringByte {
            get {
                return _UwsProducerVstringByte;
            }
            set {
                if (_UwsProducerVstringByte == value) {
                    return;
                }
                _UwsProducerVstringByte = value;
            }
        }

        public byte[] UwsProducerDdlVstringByte {
            get {
                return _UwsProducerDdlVstringByte;
            }
            set {
                if (_UwsProducerDdlVstringByte == value) {
                    return;
                }
                _UwsProducerDdlVstringByte = value;
            }
        }

        public string UwsProducerName {
            get {
                return _UwsProducerName;
            }
            set {
                if (_UwsProducerName == value) {
                    return;
                }
                _UwsProducerName = value;
            }
        }

        public string UwsProducerVstring {
            get {
                return _UwsProducerVstring;
            }
            set {
                if (_UwsProducerVstring == value) {
                    return;
                }
                _UwsProducerVstring = value;
            }
        }

        public string UwsProducerDdlVstring {
            get {
                return _UwsProducerDdlVstring;
            }
            set {
                if (_UwsProducerDdlVstring == value) {
                    return;
                }
                _UwsProducerDdlVstring = value;
            }
        }

        public int UwsProducer {
            get {
                return _UwsProducer;
            }
            set {
                if (_UwsProducer == value) {
                    return;
                }
                _UwsProducer = value;
            }
        }

        public long UwsProducerVersion {
            get {
                return _UwsProducerVersion;
            }
            set {
                if (_UwsProducerVersion == value) {
                    return;
                }
                _UwsProducerVersion = value;
            }
        }

        public long UwsProducerDdlVersion {
            get {
                return _UwsProducerDdlVersion;
            }
            set {
                if (_UwsProducerDdlVersion == value) {
                    return;
                }
                _UwsProducerDdlVersion = value;
            }
        }

        #endregion

        #region Sample Info

        private long _UwsClassTotalDataSize; //Bytes
        private long _UwsGMTStartTimestamp; //Juliantimestamp
        private long _UwsGMTStopTimestamp;
        private long _UwsLCTStartTimestamp;
        private long _UwsSampleInterval; //Microseconds

        public long UwsGMTStartTimestamp {
            get {
                return _UwsGMTStartTimestamp;
            }
            set {
                if (_UwsGMTStartTimestamp == value) {
                    return;
                }
                _UwsGMTStartTimestamp = value;
            }
        }

        public long UwsGMTStopTimestamp {
            get {
                return _UwsGMTStopTimestamp;
            }
            set {
                if (_UwsGMTStopTimestamp == value) {
                    return;
                }
                _UwsGMTStopTimestamp = value;
            }
        }

        public long UwsLCTStartTimestamp {
            get {
                return _UwsLCTStartTimestamp;
            }
            set {
                if (_UwsLCTStartTimestamp == value) {
                    return;
                }
                _UwsLCTStartTimestamp = value;
            }
        }

        public long UwsSampleInterval {
            get {
                return _UwsSampleInterval;
            }
            set {
                if (_UwsSampleInterval == value) {
                    return;
                }
                _UwsSampleInterval = value;
            }
        }

        public long UwsClassTotalDataSize {
            get {
                return _UwsClassTotalDataSize;
            }
            set {
                if (_UwsClassTotalDataSize == value) {
                    return;
                }
                _UwsClassTotalDataSize = value;
            }
        }

        #endregion

        #region Class Info

        private int _UwsClassCollectionState;
        private string _UwsClassCollectionStateString = string.Empty;
        private byte[] _UwsClassCollectionStateStringByte = new byte[128];
        private int _UwsClassCompatibleDataVersion;
        private string _UwsClassCompatibleDataVstring = string.Empty;
        private byte[] _UwsClassCompatibleDataVstringByte = new byte[64];
        private int _UwsClassCompatibleDdlVersion;
        private string _UwsClassCompatibleDdlVstring = string.Empty;
        private byte[] _UwsClassCompatibleDdlVstringByte = new byte[64];
        private int _UwsClassDataVersion;
        private string _UwsClassDataVstring = string.Empty;
        private byte[] _UwsClassDataVstringByte = new byte[64];
        private int _UwsClassDdlVersion;
        private string _UwsClassDdlVstring = string.Empty;
        private byte[] _UwsClassDdlVstringByte = new byte[64];
        private int _UwsClassId;
        private string _UwsClassName = string.Empty;
        private byte[] _UwsClassNameByte = new byte[64]; //' "Measure"
        private int _UwsClassSampleError;
        private string _UwsClassSampleErrorString = string.Empty;
        private byte[] _UwsClassSampleErrorStringByte = new byte[96];
        private int _UwsClassSampleState;
        private string _UwsClassSampleStateString = string.Empty;
        private byte[] _UwsClassSampleStateStringByte = new byte[96];
        private string _UwsSignatureType = string.Empty;

        public byte[] UwsClassNameByte {
            get {
                return _UwsClassNameByte;
            }
            set {
                if (_UwsClassNameByte == value) {
                    return;
                }
                _UwsClassNameByte = value;
            }
        }

        public byte[] UwsClassDdlVstringByte {
            get {
                return _UwsClassDdlVstringByte;
            }
            set {
                if (_UwsClassDdlVstringByte == value) {
                    return;
                }
                _UwsClassDdlVstringByte = value;
            }
        }

        public byte[] UwsClassCompatibleDdlVstringByte {
            get {
                return _UwsClassCompatibleDdlVstringByte;
            }
            set {
                if (_UwsClassCompatibleDdlVstringByte == value) {
                    return;
                }
                _UwsClassCompatibleDdlVstringByte = value;
            }
        }

        public byte[] UwsClassDataVstringByte {
            get {
                return _UwsClassDataVstringByte;
            }
            set {
                if (_UwsClassDataVstringByte == value) {
                    return;
                }
                _UwsClassDataVstringByte = value;
            }
        }

        public byte[] UwsClassCompatibleDataVstringByte {
            get {
                return _UwsClassCompatibleDataVstringByte;
            }
            set {
                if (_UwsClassCompatibleDataVstringByte == value) {
                    return;
                }
                _UwsClassCompatibleDataVstringByte = value;
            }
        }

        public byte[] UwsClassCollectionStateStringByte {
            get {
                return _UwsClassCollectionStateStringByte;
            }
            set {
                if (_UwsClassCollectionStateStringByte == value) {
                    return;
                }
                _UwsClassCollectionStateStringByte = value;
            }
        }

        public byte[] UwsClassSampleStateStringByte {
            get {
                return _UwsClassSampleStateStringByte;
            }
            set {
                if (_UwsClassSampleStateStringByte == value) {
                    return;
                }
                _UwsClassSampleStateStringByte = value;
            }
        }

        public byte[] UwsClassSampleErrorStringByte {
            get {
                return _UwsClassSampleErrorStringByte;
            }
            set {
                if (_UwsClassSampleErrorStringByte == value) {
                    return;
                }
                _UwsClassSampleErrorStringByte = value;
            }
        }

        public string UwsSignatureType {
            get {
                return _UwsSignatureType;
            }
            set {
                if (_UwsSignatureType == value) {
                    return;
                }
                _UwsSignatureType = value;
            }
        }

        public string UwsClassName {
            get {
                return _UwsClassName;
            }
            set {
                if (_UwsClassName == value) {
                    return;
                }
                _UwsClassName = value;
            }
        }

        public string UwsClassDdlVstring {
            get {
                return _UwsClassDdlVstring;
            }
            set {
                if (_UwsClassDdlVstring == value) {
                    return;
                }
                _UwsClassDdlVstring = value;
            }
        }

        public string UwsClassCompatibleDdlVstring {
            get {
                return _UwsClassCompatibleDdlVstring;
            }
            set {
                if (_UwsClassCompatibleDdlVstring == value) {
                    return;
                }
                _UwsClassCompatibleDdlVstring = value;
            }
        }

        public string UwsClassDataVstring {
            get {
                return _UwsClassDataVstring;
            }
            set {
                if (_UwsClassDataVstring == value) {
                    return;
                }
                _UwsClassDataVstring = value;
            }
        }

        public string UwsClassCompatibleDataVstring {
            get {
                return _UwsClassCompatibleDataVstring;
            }
            set {
                if (_UwsClassCompatibleDataVstring == value) {
                    return;
                }
                _UwsClassCompatibleDataVstring = value;
            }
        }

        public string UwsClassSampleStateString {
            get {
                return _UwsClassSampleStateString;
            }
            set {
                if (_UwsClassSampleStateString == value) {
                    return;
                }
                _UwsClassSampleStateString = value;
            }
        }

        public string UwsClassCollectionStateString {
            get {
                return _UwsClassCollectionStateString;
            }
            set {
                if (_UwsClassCollectionStateString == value) {
                    return;
                }
                _UwsClassCollectionStateString = value;
            }
        }

        public string UwsClassSampleErrorString {
            get {
                return _UwsClassSampleErrorString;
            }
            set {
                if (_UwsClassSampleErrorString == value) {
                    return;
                }
                _UwsClassSampleErrorString = value;
            }
        }

        public int UwsClassId {
            get {
                return _UwsClassId;
            }
            set {
                if (_UwsClassId == value) {
                    return;
                }
                _UwsClassId = value;
            }
        }

        public int UwsClassDdlVersion {
            get {
                return _UwsClassDdlVersion;
            }
            set {
                if (_UwsClassDdlVersion == value) {
                    return;
                }
                _UwsClassDdlVersion = value;
            }
        }

        public int UwsClassCompatibleDdlVersion {
            get {
                return _UwsClassCompatibleDdlVersion;
            }
            set {
                if (_UwsClassCompatibleDdlVersion == value) {
                    return;
                }
                _UwsClassCompatibleDdlVersion = value;
            }
        }

        public int UwsClassDataVersion {
            get {
                return _UwsClassDataVersion;
            }
            set {
                if (_UwsClassDataVersion == value) {
                    return;
                }
                _UwsClassDataVersion = value;
            }
        }

        public int UwsClassCompatibleDataVersion {
            get {
                return _UwsClassCompatibleDataVersion;
            }
            set {
                if (_UwsClassCompatibleDataVersion == value) {
                    return;
                }
                _UwsClassCompatibleDataVersion = value;
            }
        }

        public int UwsClassCollectionState {
            get {
                return _UwsClassCollectionState;
            }
            set {
                if (_UwsClassCollectionState == value) {
                    return;
                }
                _UwsClassCollectionState = value;
            }
        }

        public int UwsClassSampleState {
            get {
                return _UwsClassSampleState;
            }
            set {
                if (_UwsClassSampleState == value) {
                    return;
                }
                _UwsClassSampleState = value;
            }
        }

        public int UwsClassSampleError {
            get {
                return _UwsClassSampleError;
            }
            set {
                if (_UwsClassSampleError == value) {
                    return;
                }
                _UwsClassSampleError = value;
            }
        }

        #endregion

        #region Collector Data Class Info

        private int _UwsCdataClassId;
        private string _UwsCdataClassName = string.Empty;
        private byte[] _UwsCdataClassNameByte = new byte[64]; //         ' "Tpdc" or "TPDC"
        private int _UwsCdataCollectionState;
        private string _UwsCdataCollectionStateString = string.Empty;
        private byte[] _UwsCdataCollectionStateStringByte = new byte[96];
        private int _UwsCdataCompatibleDataVersion;
        private string _UwsCdataCompatibleDataVstring = string.Empty;
        private byte[] _UwsCdataCompatibleDataVstringByte = new byte[64];
        private int _UwsCdataCompatibleDdlVersion;
        private string _UwsCdataCompatibleDdlVstring = string.Empty;
        private byte[] _UwsCdataCompatibleDdlVstringByte = new byte[64];
        private int _UwsCdataDataVersion;
        private byte[] _UwsCdataDataVstringByte = new byte[64];
        private int _UwsCdataDdlVersion;
        private string _UwsCdataDdlVstring = string.Empty;
        private byte[] _UwsCdataDdlVstringByte = new byte[64];
        private int _UwsCdataSampleError;
        private string _UwsCdataSampleErrorString = string.Empty;
        private byte[] _UwsCdataSampleErrorStringByte = new byte[96];
        private int _UwsCdataSampleState;
        private string _UwsCdataSampleStateString = string.Empty;
        private byte[] _UwsCdataSampleStateStringByte = new byte[128];

        public byte[] UwsCdataClassNameByte {
            get {
                return _UwsCdataClassNameByte;
            }
            set {
                if (_UwsCdataClassNameByte == value) {
                    return;
                }
                _UwsCdataClassNameByte = value;
            }
        }

        public byte[] UwsCdataDdlVstringByte {
            get {
                return _UwsCdataDdlVstringByte;
            }
            set {
                if (_UwsCdataDdlVstringByte == value) {
                    return;
                }
                _UwsCdataDdlVstringByte = value;
            }
        }

        public byte[] UwsCdataCompatibleDdlVstringByte {
            get {
                return _UwsCdataCompatibleDdlVstringByte;
            }
            set {
                if (_UwsCdataCompatibleDdlVstringByte == value) {
                    return;
                }
                _UwsCdataCompatibleDdlVstringByte = value;
            }
        }

        public byte[] UwsCdataDataVstringByte {
            get {
                return _UwsCdataDataVstringByte;
            }
            set {
                if (_UwsCdataDataVstringByte == value) {
                    return;
                }
                _UwsCdataDataVstringByte = value;
            }
        }

        public byte[] UwsCdataCompatibleDataVstringByte {
            get {
                return _UwsCdataCompatibleDataVstringByte;
            }
            set {
                if (_UwsCdataCompatibleDataVstringByte == value) {
                    return;
                }
                _UwsCdataCompatibleDataVstringByte = value;
            }
        }

        public byte[] UwsCdataCollectionStateStringByte {
            get {
                return _UwsCdataCollectionStateStringByte;
            }
            set {
                if (_UwsCdataCollectionStateStringByte == value) {
                    return;
                }
                _UwsCdataCollectionStateStringByte = value;
            }
        }

        public byte[] UwsCdataSampleStateStringByte {
            get {
                return _UwsCdataSampleStateStringByte;
            }
            set {
                if (_UwsCdataSampleStateStringByte == value) {
                    return;
                }
                _UwsCdataSampleStateStringByte = value;
            }
        }

        public byte[] UwsCdataSampleErrorStringByte {
            get {
                return _UwsCdataSampleErrorStringByte;
            }
            set {
                if (_UwsCdataSampleErrorStringByte == value) {
                    return;
                }
                _UwsCdataSampleErrorStringByte = value;
            }
        }

        public string UwsCdataClassName {
            get {
                return _UwsCdataClassName;
            }
            set {
                if (_UwsCdataClassName == value) {
                    return;
                }
                _UwsCdataClassName = value;
            }
        }

        public string UwsCdataDdlVstring {
            get {
                return _UwsCdataDdlVstring;
            }
            set {
                if (_UwsCdataDdlVstring == value) {
                    return;
                }
                _UwsCdataDdlVstring = value;
            }
        }

        public string UwsCdataCompatibleDdlVstring {
            get {
                return _UwsCdataCompatibleDdlVstring;
            }
            set {
                if (_UwsCdataCompatibleDdlVstring == value) {
                    return;
                }
                _UwsCdataCompatibleDdlVstring = value;
            }
        }

        public string UwsCdataCompatibleDataVstring {
            get {
                return _UwsCdataCompatibleDataVstring;
            }
            set {
                if (_UwsCdataCompatibleDataVstring == value) {
                    return;
                }
                _UwsCdataCompatibleDataVstring = value;
            }
        }

        public string UwsCdataCollectionStateString {
            get {
                return _UwsCdataCollectionStateString;
            }
            set {
                if (_UwsCdataCollectionStateString == value) {
                    return;
                }
                _UwsCdataCollectionStateString = value;
            }
        }

        public string UwsCdataSampleStateString {
            get {
                return _UwsCdataSampleStateString;
            }
            set {
                if (_UwsCdataSampleStateString == value) {
                    return;
                }
                _UwsCdataSampleStateString = value;
            }
        }

        public string UwsCdataSampleErrorString {
            get {
                return _UwsCdataSampleErrorString;
            }
            set {
                if (_UwsCdataSampleErrorString == value) {
                    return;
                }
                _UwsCdataSampleErrorString = value;
            }
        }

        public int UwsCdataClassId {
            get {
                return _UwsCdataClassId;
            }
            set {
                if (_UwsCdataClassId == value) {
                    return;
                }
                _UwsCdataClassId = value;
            }
        }

        public int UwsCdataDdlVersion {
            get {
                return _UwsCdataDdlVersion;
            }
            set {
                if (_UwsCdataDdlVersion == value) {
                    return;
                }
                _UwsCdataDdlVersion = value;
            }
        }

        public int UwsCdataCompatibleDdlVersion {
            get {
                return _UwsCdataCompatibleDdlVersion;
            }
            set {
                if (_UwsCdataCompatibleDdlVersion == value) {
                    return;
                }
                _UwsCdataCompatibleDdlVersion = value;
            }
        }

        public int UwsCdataDataVersion {
            get {
                return _UwsCdataDataVersion;
            }
            set {
                if (_UwsCdataDataVersion == value) {
                    return;
                }
                _UwsCdataDataVersion = value;
            }
        }

        public int UwsCdataCompatibleDataVersion {
            get {
                return _UwsCdataCompatibleDataVersion;
            }
            set {
                if (_UwsCdataCompatibleDataVersion == value) {
                    return;
                }
                _UwsCdataCompatibleDataVersion = value;
            }
        }

        public int UwsCdataCollectionState {
            get {
                return _UwsCdataCollectionState;
            }
            set {
                if (_UwsCdataCollectionState == value) {
                    return;
                }
                _UwsCdataCollectionState = value;
            }
        }

        public int UwsCdataSampleState {
            get {
                return _UwsCdataSampleState;
            }
            set {
                if (_UwsCdataSampleState == value) {
                    return;
                }
                _UwsCdataSampleState = value;
            }
        }

        public int UwsCdataSampleError {
            get {
                return _UwsCdataSampleError;
            }
            set {
                if (_UwsCdataSampleError == value) {
                    return;
                }
                _UwsCdataSampleError = value;
            }
        }

        #endregion

        #region Collector Info

        private int _UwsAccessorId;
        private string _UwsAccessorName = string.Empty;
        private byte[] _UwsAccessorNameByte = new byte[64];
        private string _UwsCdataDataVstring = string.Empty;
        private string _UwsCollectorName = string.Empty;
        private byte[] _UwsCollectorNameByte = new byte[64];
        private int _UwsCollectorVersion;
        private string _UwsCollectorVstring = string.Empty;
        private byte[] _UwsCollectorVstringByte = new byte[64];
        private int _UwsCreatorId;
        private byte[] _UwsCreatorNameByte = new byte[64];
        private int _UwsDelaySeconds;
        private string _UwsHomeTerminal = string.Empty;
        private byte[] _UwsHomeTerminalByte = new byte[48];
        private long _UwsLaunchTimestamp; // Juliantimestamp
        private int _UwsPriority;
        private string _UwsProcessName = string.Empty;
        private byte[] _UwsProcessNameByte = new byte[64];
        private string _UwsProgramFile = string.Empty;
        private byte[] _UwsProgramFileByte = new byte[64];
        private string _UwsRunString = string.Empty;
        private byte[] _UwsRunStringByte = new byte[240];
        private int _UwsSamples;
        private string _UwsSwapVolume = string.Empty;
        private byte[] _UwsSwapVolumeByte = new byte[64];

        public byte[] UwsCollectorNameByte {
            get {
                return _UwsCollectorNameByte;
            }
            set {
                if (_UwsCollectorNameByte == value) {
                    return;
                }
                _UwsCollectorNameByte = value;
            }
        }

        public byte[] UwsCollectorVstringByte {
            get {
                return _UwsCollectorVstringByte;
            }
            set {
                if (_UwsCollectorVstringByte == value) {
                    return;
                }
                _UwsCollectorVstringByte = value;
            }
        }

        public byte[] UwsCreatorNameByte {
            get {
                return _UwsCreatorNameByte;
            }
            set {
                if (_UwsCreatorNameByte == value) {
                    return;
                }
                _UwsCreatorNameByte = value;
            }
        }

        public byte[] UwsAccessorNameByte {
            get {
                return _UwsAccessorNameByte;
            }
            set {
                if (_UwsAccessorNameByte == value) {
                    return;
                }
                _UwsAccessorNameByte = value;
            }
        }

        public byte[] UwsRunStringByte {
            get {
                return _UwsRunStringByte;
            }
            set {
                if (_UwsRunStringByte == value) {
                    return;
                }
                _UwsRunStringByte = value;
            }
        }

        public byte[] UwsProcessNameByte {
            get {
                return _UwsProcessNameByte;
            }
            set {
                if (_UwsProcessNameByte == value) {
                    return;
                }
                _UwsProcessNameByte = value;
            }
        }

        public byte[] UwsProgramFileByte {
            get {
                return _UwsProgramFileByte;
            }
            set {
                if (_UwsProgramFileByte == value) {
                    return;
                }
                _UwsProgramFileByte = value;
            }
        }

        public byte[] UwsHomeTerminalByte {
            get {
                return _UwsHomeTerminalByte;
            }
            set {
                if (_UwsHomeTerminalByte == value) {
                    return;
                }
                _UwsHomeTerminalByte = value;
            }
        }

        public byte[] UwsSwapVolumeByte {
            get {
                return _UwsSwapVolumeByte;
            }
            set {
                if (_UwsSwapVolumeByte == value) {
                    return;
                }
                _UwsSwapVolumeByte = value;
            }
        }

        public string UwsCdataDataVstring {
            get {
                return _UwsCdataDataVstring;
            }
            set {
                if (_UwsCdataDataVstring == value) {
                    return;
                }
                _UwsCdataDataVstring = value;
            }
        }

        public string UwsCollectorName {
            get {
                return _UwsCollectorName;
            }
            set {
                if (_UwsCollectorName == value) {
                    return;
                }
                _UwsCollectorName = value;
            }
        }

        public string UwsCollectorVstring {
            get {
                return _UwsCollectorVstring;
            }
            set {
                if (_UwsCollectorVstring == value) {
                    return;
                }
                _UwsCollectorVstring = value;
            }
        }

        public string UwsAccessorName {
            get {
                return _UwsAccessorName;
            }
            set {
                if (_UwsAccessorName == value) {
                    return;
                }
                _UwsAccessorName = value;
            }
        }

        public string UwsRunString {
            get {
                return _UwsRunString;
            }
            set {
                if (_UwsRunString == value) {
                    return;
                }
                _UwsRunString = value;
            }
        }

        public string UwsProcessName {
            get {
                return _UwsProcessName;
            }
            set {
                if (_UwsProcessName == value) {
                    return;
                }
                _UwsProcessName = value;
            }
        }

        public string UwsProgramFile {
            get {
                return _UwsProgramFile;
            }
            set {
                if (_UwsProgramFile == value) {
                    return;
                }
                _UwsProgramFile = value;
            }
        }

        public string UwsHomeTerminal {
            get {
                return _UwsHomeTerminal;
            }
            set {
                if (_UwsHomeTerminal == value) {
                    return;
                }
                _UwsHomeTerminal = value;
            }
        }

        public string UwsSwapVolume {
            get {
                return _UwsSwapVolume;
            }
            set {
                if (_UwsSwapVolume == value) {
                    return;
                }
                _UwsSwapVolume = value;
            }
        }

        public int UwsCollectorVersion {
            get {
                return _UwsCollectorVersion;
            }
            set {
                if (_UwsCollectorVersion == value) {
                    return;
                }
                _UwsCollectorVersion = value;
            }
        }

        public int UwsCreatorId {
            get {
                return _UwsCreatorId;
            }
            set {
                if (_UwsCreatorId == value) {
                    return;
                }
                _UwsCreatorId = value;
            }
        }

        public int UwsAccessorId {
            get {
                return _UwsAccessorId;
            }
            set {
                if (_UwsAccessorId == value) {
                    return;
                }
                _UwsAccessorId = value;
            }
        }

        public int UwsPriority {
            get {
                return _UwsPriority;
            }
            set {
                if (_UwsPriority == value) {
                    return;
                }
                _UwsPriority = value;
            }
        }

        public int UwsSamples {
            get {
                return _UwsSamples;
            }
            set {
                if (_UwsSamples == value) {
                    return;
                }
                _UwsSamples = value;
            }
        }

        public int UwsDelaySeconds {
            get {
                return _UwsDelaySeconds;
            }
            set {
                if (_UwsDelaySeconds == value) {
                    return;
                }
                _UwsDelaySeconds = value;
            }
        }

        public long UwsLaunchTimestamp {
            get {
                return _UwsLaunchTimestamp;
            }
            set {
                if (_UwsLaunchTimestamp == value) {
                    return;
                }
                _UwsLaunchTimestamp = value;
            }
        }

        #endregion
    }
}
