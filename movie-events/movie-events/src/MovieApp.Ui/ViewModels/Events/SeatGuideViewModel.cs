using System;
using System.Collections.ObjectModel;
using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class SeatGuideViewModel : ViewModelBase
{
    public ObservableCollection<Seat> Seats { get; } = new();

    public int TotalRows { get; private set; }
    public int TotalColumns { get; private set; }
    
    public SeatGuideViewModel(int totalCapacity = 50)
    {
        GenerateDynamicLayout(totalCapacity);
    }

    private void GenerateDynamicLayout(int capacity)
    {
        Seats.Clear();
        
        TotalColumns = 10;
        
        TotalRows = (int)Math.Ceiling((double)capacity / TotalColumns);
        
        int centerRow = TotalRows / 2 + 1; 
        int centerCol = TotalColumns / 2;

        int seatCount = 0;

        for (int row = 1; row <= TotalRows; row++)
        {
            for (int col = 1; col <= TotalColumns; col++)
            {

                if (seatCount >= capacity) break;

                var seat = new Seat { Row = row, Column = col };

                if (row <= 2 && TotalRows > 3)
                {
                    seat.Quality = SeatQuality.Poor;
                }
                else if (row == 1 && TotalRows <= 3)
                {
                    seat.Quality = SeatQuality.Poor;
                }

                else if (Math.Abs(row - centerRow) <= 1 && Math.Abs(col - centerCol) <= 1)
                {
                    seat.Quality = SeatQuality.Optimal;
                    seat.IsSweetSpot = true;
                }
                else
                {
                    seat.Quality = SeatQuality.Standard;
                }
                
                if (Random.Shared.Next(100) < 15) 
                {
                    seat.IsAvailable = false;
                }

                Seats.Add(seat);
                seatCount++;
            }
        }
    }
}