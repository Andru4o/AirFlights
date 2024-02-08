using System.Data;
using AirFlights.Domain;
using FluentValidation;

namespace AirFlights.Infrastructure;

public class FlightValidator : AbstractValidator<Flight> 
{
    public FlightValidator()
    {
        RuleFor(flight => flight.Origin).NotEmpty().WithMessage("Origin can't be empty");
        RuleFor(flight => flight.Origin).NotNull().WithMessage("Origin can't be null");
        RuleFor(flight => flight.Origin).MaximumLength(256).WithMessage("Origin's length more than 256 characters");
        RuleFor(flight => flight.Destination).NotEmpty().WithMessage("Destination can't be empty");
        RuleFor(flight => flight.Destination).NotNull().WithMessage("Destination can't be null");
        RuleFor(flight => flight.Destination).MaximumLength(256).WithMessage("Destination's length more than 256 characters");
        RuleFor(flight => flight.Departure).NotNull().WithMessage("Departure can't be null");
        RuleFor(flight => flight.Status).NotNull().WithMessage("Status can't be null");
    }
}