namespace Tenet.Domain.Configuration
{
    public class DriverConfiguration
    {
        public int Pid { get; set; }
        public string Name { get; set; }
        public string Bytes { get; set; }
        public string Secret { get; set; }
        public double Expiry { get; set; }
    }
}
