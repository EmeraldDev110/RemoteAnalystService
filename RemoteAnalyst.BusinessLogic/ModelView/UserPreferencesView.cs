namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class UserPreferencesView
    {
        private short _NotifyAll = 1;
        private short _NotifyCritical = 1;
        private short _NotifyInfo = 1;
        private short _NotifyMajor = 1;
        private short _NotifyMinor = 1;
        private short _NotifyPrivate = 1;
        private short _NotifyPublic = 1;
        private short _NotifyWarn = 1;

        public short NotifyAll
        {
            get { return _NotifyAll; }
            set { _NotifyAll = value; }
        }

        public short NotifyCritical
        {
            get { return _NotifyCritical; }
            set { _NotifyCritical = value; }
        }

        public short NotifyMajor
        {
            get { return _NotifyMajor; }
            set { _NotifyMajor = value; }
        }

        public short NotifyMinor
        {
            get { return _NotifyMinor; }
            set { _NotifyMinor = value; }
        }

        public short NotifyWarn
        {
            get { return _NotifyWarn; }
            set { _NotifyWarn = value; }
        }

        public short NotifyInfo
        {
            get { return _NotifyInfo; }
            set { _NotifyInfo = value; }
        }

        public short NotifyPrivate
        {
            get { return _NotifyPrivate; }
            set { _NotifyPrivate = value; }
        }

        public short NotifyPublic
        {
            get { return _NotifyPublic; }
            set { _NotifyPublic = value; }
        }
    }
}