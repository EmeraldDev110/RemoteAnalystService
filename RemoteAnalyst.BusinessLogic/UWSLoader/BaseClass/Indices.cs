using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass {
    public class Indices : IndicesV1 {
        private long _FInterval;
        private string _FMeasVer = string.Empty;
        private string _FName = string.Empty;
        //private byte _FType = 0;
        private short _FReclen;
        private int _FRecords;
        private int _FStartDay;
        private long _FStartTime;
        private int _FStopDay;
        private long _FStopTime;
        private string _FSysName = string.Empty;
        private short _FType;
        private long _FilePosition;

        public string FName {
            get {
                return _FName;
            }
            set {
                if (_FName == value) {
                    return;
                }
                _FName = value;
            }
        }

        public string FSysName {
            get {
                return _FSysName;
            }
            set {
                if (_FSysName == value) {
                    return;
                }
                _FSysName = value;
            }
        }

        public short FType {
            get {
                return _FType;
            }
            set {
                if (_FType == value) {
                    return;
                }
                _FType = value;
            }
        }

        public short FReclen {
            get {
                return _FReclen;
            }
            set {
                if (_FReclen == value) {
                    return;
                }
                _FReclen = value;
            }
        }

        public string FMeasVer {
            get {
                return _FMeasVer;
            }
            set {
                if (_FMeasVer == value) {
                    return;
                }
                _FMeasVer = value;
            }
        }

        public int FRecords {
            get {
                return _FRecords;
            }
            set {
                if (_FRecords == value) {
                    return;
                }
                _FRecords = value;
            }
        }

        public int FStartDay {
            get {
                return _FStartDay;
            }
            set {
                if (_FStartDay == value) {
                    return;
                }
                _FStartDay = value;
            }
        }

        public int FStopDay {
            get {
                return _FStopDay;
            }
            set {
                if (_FStopDay == value) {
                    return;
                }
                _FStopDay = value;
            }
        }

        public long FilePosition {
            get {
                return _FilePosition;
            }
            set {
                if (_FilePosition == value) {
                    return;
                }
                _FilePosition = value;
            }
        }

        public long FStartTime {
            get {
                return _FStartTime;
            }
            set {
                if (_FStartTime == value) {
                    return;
                }
                _FStartTime = value;
            }
        }

        public long FStopTime {
            get {
                return _FStopTime;
            }
            set {
                if (_FStopTime == value) {
                    return;
                }
                _FStopTime = value;
            }
        }

        public long FInterval {
            get {
                return _FInterval;
            }
            set {
                if (_FInterval == value) {
                    return;
                }
                _FInterval = value;
            }
        }
    }
}
