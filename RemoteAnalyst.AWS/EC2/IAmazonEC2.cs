using System;
using System.Collections.Generic;

namespace RemoteAnalyst.AWS.EC2
{
    public interface IAmazonEC2
    {
        void TerminateEC2Instance(List<string> instanceIDs);

        DateTime GetLaunchTime(string instanceId);

        string GetEC2ID();
    }
}