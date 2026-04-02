namespace MovieApp.Core.Models;

public enum SeatQuality
{
    Poor,   
    Standard,  
    Optimal     
}

public sealed class Seat
{
    public int Row { get; set; }
    public int Column { get; set; }
    public SeatQuality Quality { get; set; }
    
    public bool IsSweetSpot { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public string SeatColor => Quality switch
    {
        SeatQuality.Poor => "#FF4D4D",    
        SeatQuality.Optimal => "#4CAF50",  
        SeatQuality.Standard => "#FFC107",  
        _ => "#E0E0E0"
    };
}