using System.Data.Linq.Mapping;

// Ref : https://docs.microsoft.com/en-us/visualstudio/cross-platform/unity-scripting-upgrade?view=vs-2017
// Using .NET 4.x in Unity

[Table]
public class Data
{
    [Column(IsPrimaryKey=true)]
    public string SlotID;

    [Column]
    public string Region;

    [Column(Name = "Casino Name")]
    public string CasinoName;

    [Column]
    public string Theme;

    [Column]
    public float Denom;

    [Column]
    public string Manufacturer;

    [Column(Name = "Days on Floor")]
    public int DaysOnFloor;

    [Column(Name = "Games Played per Day")]
    public int GamesPlayedPerDay;

    [Column(Name = "Average Bet")]
    public float AverageBet;

    [Column]
    public float Occupancy;


    public override string ToString()
    {
        return "SlotID: " + SlotID + "\n" +
            "Region: " + Region + "\n" +
            "Casino Name: " + CasinoName + "\n" +
            "Theme: " + Theme + "\n" +
            "Denom: " + Denom + "\n" +
            "Manufacturer: " + Manufacturer + "\n" +
            "DaysOnFloor: " + DaysOnFloor + "\n" +
            "Games Played PerDay: " + GamesPlayedPerDay + "\n" +
            "AverageBet: " + AverageBet + "\n" +
            "Occupancy: " + Occupancy;
    }
}