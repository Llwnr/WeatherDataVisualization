namespace WeatherApi.Helper;
public static class UrlService{
    private static IConfiguration _configuration;
    public static int NumOfFiles => _configuration.GetSection("NomadsUrlConfig:NumOfFiles").Get<int>();
    public static void SetConfiguration(IConfiguration configuration){
        _configuration = configuration;
    }
    
    public static List<string> GenerateForecastUrls(DateTime startingTime, int numOfUrls, List<string>? parameters = null, List<string>? levels = null){
        if (_configuration == null){
            throw new InvalidOperationException("Configuration not set up");
        }
        List<string> fileUrls = new List<string>();
        string baseUrl = _configuration.GetSection("NomadsUrlConfig:BaseUrl").Value;
        string directoryFormat = _configuration.GetSection("NomadsUrlConfig:DirectoryFormat").Value;
        string fileFormat = _configuration.GetSection("NomadsUrlConfig:FileFormat").Value;
        string quarterlyHour = "00";//For now always start from first hour of the new day
        parameters ??= _configuration.GetSection("NomadsUrlConfig:DefaultParameters").Get<List<String>>();
        levels ??= _configuration.GetSection("NomadsUrlConfig:DefaultLevels").Get<List<String>>();
        int startingHour = GetStartingHour(startingTime);
        for (int i = startingHour+1; i <= numOfUrls+startingHour; i++){
            string dir = directoryFormat
                .Replace("{date}", DateTime.UtcNow.ToString("yyyyMMdd"))
                .Replace("{hour}", quarterlyHour);
            string filename = fileFormat
                .Replace("{hour}", quarterlyHour)
                .Replace("{forecastHour}", i.ToString("D3"));
            string finalUrl = $"{baseUrl}?dir={dir}&file={filename}";

            foreach (var parameter in parameters){
                finalUrl += $"&var_{parameter}=on";
            }
            foreach (var level in levels){
                string level_parsed = level.Replace(" ", "_");
                finalUrl += $"&lev_{level_parsed}=on";
            }
            fileUrls.Add(finalUrl);
        }
        return fileUrls;
    }

    static int GetStartingHour(DateTime startingForecastHour){
        DateTime currentDate = DateTime.UtcNow;
        DateTime currentDay = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);
        
        TimeSpan hourDiff = startingForecastHour - currentDay;
        return (int)hourDiff.TotalHours;
    }
    
    static string GetQuaterlyHours(DateTime time){
        int hour = int.Parse(time.ToString("hh"));
        if (hour > 18) hour = 18;
        else if (hour > 12) hour = 12;
        else if (hour > 6) hour = 6;
        else hour = 0;
        return hour.ToString("D2");
    }
}