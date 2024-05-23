using System;
using System.Collections.Generic;

namespace RemoteAnalyst.BusinessLogic.Infrastructure
{
    public class LoadedTime
    {
        public DateTime LoadedStartTime { get; set; }
        public DateTime LoadedStopTime { get; set; }

        public List<LoadedTime> MergeContinuesTime(List<LoadedTime> loadedTimes)
        {
            var mergedTime = new List<LoadedTime>();

            foreach (LoadedTime lt in loadedTimes)
            {
                if (mergedTime.Count == 0)
                {
                    var loadTime = new LoadedTime();
                    loadTime.LoadedStartTime = lt.LoadedStartTime;
                    loadTime.LoadedStopTime = lt.LoadedStopTime;

                    mergedTime.Add(loadTime);
                }
                else
                {
                    var loadTime = new LoadedTime();
                    bool noMerge = false;
                    foreach (LoadedTime mlt in mergedTime)
                    {
                        if (mlt.LoadedStartTime >= lt.LoadedStartTime && mlt.LoadedStartTime <= lt.LoadedStopTime)
                        {
                            //loadInfo.UpdateData(UWSSerialNumber, dataStartDate, dataStopDate, lt.LoadedStartTime, selectedStopTime);
                            //mlt.LoadedStopTime = lt.LoadedStopTime;
                            mlt.LoadedStartTime = lt.LoadedStartTime;
                            noMerge = true;
                        }
                        else if (mlt.LoadedStopTime >= lt.LoadedStartTime && mlt.LoadedStopTime <= lt.LoadedStopTime)
                        {
                            //loadInfo.UpdateData(UWSSerialNumber, dataStartDate, dataStopDate, selectedStartTime, lt.LoadedStopTime);
                            //mlt.LoadedStartTime = lt.LoadedStartTime;
                            mlt.LoadedStopTime = lt.LoadedStopTime;
                            noMerge = true;
                        }
                        else
                        {
                            if (lt.LoadedStartTime >= mlt.LoadedStartTime && lt.LoadedStopTime <= lt.LoadedStopTime)
                            {
                                noMerge = true;
                            }
                            loadTime.LoadedStartTime = lt.LoadedStartTime;
                            loadTime.LoadedStopTime = lt.LoadedStopTime;
                        }
                    }

                    if ((loadTime.LoadedStartTime != null && loadTime.LoadedStopTime != null) && !noMerge)
                        mergedTime.Add(loadTime);
                }
            }

            return mergedTime;
        }
    }
}