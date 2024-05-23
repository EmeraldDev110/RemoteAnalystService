using System;

namespace RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass {
    public class MultiDays {
        private bool _DontLoad;
        private DateTime _EndDate;
        private int _EntityID;
        private long _Interval;
        private string _MeasureVersion = string.Empty;
        private DateTime _StartDate;
        private string _SystemSerial = string.Empty;
        private string _TableName = string.Empty;

        public string TableName {
            get {
                return _TableName;
            }
            set {
                if (_TableName == value) {
                    return;
                }
                _TableName = value;
            }
        }

        public int EntityID {
            get {
                return _EntityID;
            }
            set {
                if (_EntityID == value) {
                    return;
                }
                _EntityID = value;
            }
        }

        public string SystemSerial {
            get {
                return _SystemSerial;
            }
            set {
                if (_SystemSerial == value) {
                    return;
                }
                _SystemSerial = value;
            }
        }

        public long Interval {
            get {
                return _Interval;
            }
            set {
                if (_Interval == value) {
                    return;
                }
                _Interval = value;
            }
        }

        public string MeasureVersion {
            get {
                return _MeasureVersion;
            }
            set {
                if (_MeasureVersion == value) {
                    return;
                }
                _MeasureVersion = value;
            }
        }

        public DateTime StartDate {
            get {
                return _StartDate;
            }
            set {
                if (_StartDate == value) {
                    return;
                }
                _StartDate = value;
            }
        }

        public DateTime EndDate {
            get {
                return _EndDate;
            }
            set {
                if (_EndDate == value) {
                    return;
                }
                _EndDate = value;
            }
        }

        public bool DontLoad {
            get {
                return _DontLoad;
            }
            set {
                if (_DontLoad == value) {
                    return;
                }
                _DontLoad = value;
            }
        }

        public override int GetHashCode() {
            return StartDate.GetHashCode() + EndDate.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            var multiDay = obj as MultiDays;
            return multiDay != null && (StartDate.Equals(multiDay.StartDate) && EndDate.Equals(multiDay.EndDate));
        }
    }
}
