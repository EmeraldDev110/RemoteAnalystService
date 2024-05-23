using System;

namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class ColumnInfoView
    {
        private string _ColumnName = String.Empty;
        private string _TestValue = String.Empty;
        private string _TypeName = String.Empty;
        private int _TypeValue;
        public bool Website { get; set; }

        public string ColumnName
        {
            get { return _ColumnName; }
            set
            {
                if (_ColumnName == value)
                    return;
                _ColumnName = value;
            }
        }

        public string TypeName
        {
            get { return _TypeName; }
            set
            {
                if (_TypeName == value)
                    return;
                _TypeName = value;
            }
        }

        public int TypeValue
        {
            get { return _TypeValue; }
            set
            {
                if (_TypeValue == value)
                    return;
                _TypeValue = value;
            }
        }

        public string TestValue
        {
            get { return _TestValue; }
            set
            {
                if (_TestValue == value)
                    return;
                _TestValue = value;
            }
        }
    }
}