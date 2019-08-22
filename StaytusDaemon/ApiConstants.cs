namespace StaytusDaemon
{
    public static class ApiConstants
    {
        public static readonly string SetStatusEndpoint = "/services/set_status";

        public static readonly string GetStatusEndpoint = "/services/info";

        public static readonly string McApi = "https://mcapi.us/server/status?ip={0}&port={1}";
        
        public static readonly Permalinks Permalinks = new Permalinks();
    }
}