using System;
using System.IO;
using Amazon;
using Amazon.AutoScaling.Model;
using Amazon.Glacier;
using Amazon.Glacier.Model;
// Add using statements to access AWS SDK for .NET services. 
// Both the Service and its Model namespace need to be added 
// in order to gain access to a service. For example, to access
// the EC2 service, add:
// using Amazon.EC2;
// using Amazon.EC2.Model;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.Infrastructure;
//using AmazonGlacier = Amazon.Glacier.AmazonGlacier;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.S3;

namespace RemoteAnalyst.AWS
{
    internal class Program
    {
        public static void Main(string[] args)
        {

        }
    }
}