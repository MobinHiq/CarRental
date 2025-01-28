using System.Collections.Generic;
using MediatR;
using CarRental.Domain.Entities;

namespace CarRental.Application.Queries;

public class GetAllRentalsQuery : IRequest<IEnumerable<Rental>>
{
} 