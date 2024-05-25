using System.Collections.Generic;
using System.Data;

namespace RemoteAnalyst.Repository.Domain
{
    public interface ISystemRepository
    {
        Dictionary<string, string> GetLicenseDate();
        string GetCompanyName(string systemSerial);
        int GetCompanyID(string systemSerial);
        int GetRetentionDay(string systemSerial);
        IDictionary<string, string> GetEndDate(string systemSerial);
        string GetMeasFH(string systemSerial);
        string GetSystemName(string systemSerial);
        bool GetAttachmentInEmail(string systemSerial);
        Dictionary<string, string> GetExpiredSystem(bool isLocalAnalyst = false);
        DataTable GetAllSystems();
        bool AllowOverlappingData(string systemSerial);
        bool isProcessDirectlySystem(string systemSerial);
        int GetTimeZone(string systemSerial);
        string GetCountryCode(string systemSerial);
        int GetArchiveRetensionValue(string systemSerial);
        int GetTrendMonth(string systemSerial);
        bool IsNTSSystem(string systemSerial);
        DataTable GetTolerance(string systemSerial);
        DataTable GetAllCompanySystemSerialAndName();
        int GetLoadLimit(string systemSerial);
    }
}
