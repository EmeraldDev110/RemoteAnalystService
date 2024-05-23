namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class CPUProcessorsService
    {
        private string ConnectionString = "";

        public CPUProcessorsService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /*public CPUProcessorsService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public int GetCpuTotalFor(string systemSerial) {

        }

        public void CheckCPUMappingFor(string systemSerial, int cpuNumber, int procID) {

        }

        public void InsertNewCPUProcessorsFor(string systemSerial, int cpuNumber, int procID) {

        }

        public void UpdateCPUProcessorsFor(string systemSerial, int cpuNumber, int procID) {

        }

        public bool CheckUpdateFor(string systemSerial, int cpuNumber) {

        }

        public void DeleteCPUMappingFor(string systemSerial, int cpuNumber) {

        }*/
    }
}