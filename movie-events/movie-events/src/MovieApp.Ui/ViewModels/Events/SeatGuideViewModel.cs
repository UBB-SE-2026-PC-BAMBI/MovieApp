// <copyright file="SeatGuideViewModel.cs" company="MovieApp">
// Copyright (c) MovieApp. All rights reserved.
// </copyright>

namespace MovieApp.Ui.ViewModels.Events;

using System;
using System.Collections.ObjectModel;
using MovieApp.Core.Models;

/// <summary>
/// Manages the layout and generation of seating for an event guide screen.
/// </summary>
public sealed class SeatGuideViewModel : ViewModelBase
{
    private const int DefaultColumnCount = 10;
    private const int UnavailabilityPercentage = 15;
    private const int PercentageBase = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeatGuideViewModel"/> class.
    /// </summary>
    /// <param name="totalCapacity">The total capacity of the venue.</param>
    public SeatGuideViewModel(int totalCapacity = Event.DefaultMaxCapacity)
    {
        this.GenerateDynamicLayout(totalCapacity);
    }

    /// <summary>
    /// Gets the collection of seats generated for the layout.
    /// </summary>
    public ObservableCollection<Seat> Seats { get; } = new ObservableCollection<Seat>();

    /// <summary>
    /// Gets the total number of rows in the layout.
    /// </summary>
    public int TotalRows { get; private set; }

    /// <summary>
    /// Gets the total number of columns in the layout.
    /// </summary>
    public int TotalColumns { get; private set; }

    private void GenerateDynamicLayout(int capacity)
    {
        this.Seats.Clear();

        this.TotalColumns = DefaultColumnCount;
        this.TotalRows = (int)Math.Ceiling((double)capacity / this.TotalColumns);

        int centerRow = (this.TotalRows / 2) + 1;
        int centerColumn = this.TotalColumns / 2;

        int currentSeatCount = 0;

        for (int row = 1; row <= this.TotalRows; row++)
        {
            for (int column = 1; column <= this.TotalColumns; column++)
            {
                if (currentSeatCount >= capacity)
                {
                    break;
                }

                Seat seat = new Seat { Row = row, Column = column };

                if (row <= 2 && this.TotalRows > 3)
                {
                    seat.Quality = SeatQuality.Poor;
                }
                else if (row == 1 && this.TotalRows <= 3)
                {
                    seat.Quality = SeatQuality.Poor;
                }
                else if (Math.Abs(row - centerRow) <= 1 && Math.Abs(column - centerColumn) <= 1)
                {
                    seat.Quality = SeatQuality.Optimal;
                    seat.IsSweetSpot = true;
                }
                else
                {
                    seat.Quality = SeatQuality.Standard;
                }

                if (Random.Shared.Next(PercentageBase) < UnavailabilityPercentage) 
                {
                    seat.IsAvailable = false;
                }

                this.Seats.Add(seat);
                currentSeatCount++;
            }
        }
    }
}