using System.Reflection;

namespace Minimal.Api;

public class MapPoint
{
    public double Latitude { get; set; }

    public double Longtitude { get; set; }

    public static bool TryParse(string? value, out MapPoint? point)
    {
        try
        {
            var splitValue = value?.Split(',').Select(double.Parse).ToArray();
            point = new MapPoint
            {
                Latitude = splitValue[0],
                Longtitude = splitValue[1]
            };
            return true;
        }
        catch (Exception)
        {
            point = null;
            return false;
        }
    }

    public static async ValueTask<MapPoint?> BindAsync(HttpContext context, ParameterInfo info)
    {
        var rewRequestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        
        try
        {
            var splitValue = rewRequestBody?.Split(',').Select(double.Parse).ToArray();
            return new MapPoint
            {
                Latitude = splitValue[0],
                Longtitude = splitValue[1]
            };
        }
        catch (Exception)
        {
            return null;
        }
    }
}