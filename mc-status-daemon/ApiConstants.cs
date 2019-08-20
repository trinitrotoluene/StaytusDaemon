namespace mc_status_daemon
{
    public static class ApiConstants
    {
        public static string SetStatusEndpoint = "/services/set_status";

        public static string GetStatusEndpoint = "/services/info";

        public static string McApi = "https://mcapi.us/server/status?ip={0}&port={1}";
        
        public static Permalinks Permalinks = new Permalinks();
    }
}