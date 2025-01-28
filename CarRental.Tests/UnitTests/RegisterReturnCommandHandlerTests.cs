using Moq;
using CarRental.Application.Handlers;
using CarRental.Application.Commands;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using CarRental.Domain.PriceCalculationStrategies;
using CarRental.Infrastructure.Interfaces;
using AutoFixture;
using CarRental.Application.Exceptions;
using CarRental.Domain;

namespace CarRental.Tests.UnitTests
{
    public class RegisterReturnCommandHandlerTests : IDisposable
    {
        private readonly Mock<IRentalRepository> _rentalRepository;
        private readonly Mock<IRedisCacheService> _redisCacheService;
        private readonly Mock<IPriceCalculationStrategyFactory> _priceCalculationStrategyFactory;
        private readonly Mock<ILogger<RegisterReturnCommandHandler>> _logger;
        private readonly RegisterReturnCommandHandler _sut; // System Under Test
        private readonly Fixture _fixture;

        public RegisterReturnCommandHandlerTests()
        {
            _fixture = new Fixture();
            _rentalRepository = new Mock<IRentalRepository>();
            _redisCacheService = new Mock<IRedisCacheService>();
            _priceCalculationStrategyFactory = new Mock<IPriceCalculationStrategyFactory>();
            _logger = new Mock<ILogger<RegisterReturnCommandHandler>>();

            _sut = new RegisterReturnCommandHandler(
                _rentalRepository.Object,
                _redisCacheService.Object,
                _priceCalculationStrategyFactory.Object,
                _logger.Object
            );
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Theory]
        [InlineData(CarCategory.Small, 500)]     // baseDayRental * numberOfDays = 100 * 5 = 500 (no km charge for small cars)
        [InlineData(CarCategory.Medium, 5650)]   // (baseDayRental * numberOfDays * 1.3) + (baseKmPrice * numberOfKm) = (100 * 5 * 1.3) + (10 * 500) = 650 + 5000 = 5650
        [InlineData(CarCategory.Large, 5750)]    // (baseDayRental * numberOfDays * 1.5) + (baseKmPrice * numberOfKm) = (100 * 5 * 1.5) + (10 * 500) = 750 + 5000 = 5750
        public async Task Handle_WithValidCommand_ShouldCalculateCorrectPrice(CarCategory category, decimal expectedPrice)
        {
            // Arrange
            var pickupDate = new DateTime(2023, 10, 1);
            var returnDate = new DateTime(2023, 10, 6);
            var bookingNumber = _fixture.Create<string>();

            var rental = new Rental
            {
                BookingNumber = bookingNumber,
                PickupDate = pickupDate,
                PickupMeterReading = 1000,
                CarCategory = category
            };

            IPriceCalculationStrategy priceStrategy = category switch
            {
                CarCategory.Small => new SmallCarPriceCalculation(),
                CarCategory.Medium => new MediumCarPriceCalculation(),
                CarCategory.Large => new LargeCarPriceCalculation(),
                _ => throw new ArgumentOutOfRangeException(nameof(category))
            };

            _rentalRepository.Setup(r => r.GetByBookingNumberAsync(bookingNumber))
                .ReturnsAsync(rental);
            _redisCacheService.Setup(r => r.GetCachedRentalAsync(bookingNumber))
                .ReturnsAsync((Rental)null);
            _priceCalculationStrategyFactory
                .Setup(f => f.GetStrategy(category))
                .Returns(priceStrategy);

            var command = new RegisterReturnCommand
            {
                BookingNumber = bookingNumber,
                ReturnDate = returnDate,
                ReturnMeterReading = 1500
            };

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ValidationErrors.Should().BeEmpty();
            result.Data.Should().NotBeNull();
            result.Data.CalculatedPrice.Should().Be(expectedPrice);
            result.Data.BookingNumber.Should().Be(bookingNumber);
            result.Data.ReturnDate.Should().Be(returnDate);
            result.Data.ReturnMeterReading.Should().Be(1500);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Handle_WithInvalidBookingNumber_ShouldThrowNotFoundException(string bookingNumber)
        {
            // Arrange
            var request = new RegisterReturnCommand
            {
                BookingNumber = bookingNumber,
                ReturnDate = DateTime.Now,
                ReturnMeterReading = 2000
            };

            _redisCacheService.Setup(x => x.GetCachedRentalAsync(bookingNumber))
                .ReturnsAsync((Rental)null);
            _rentalRepository.Setup(x => x.GetByBookingNumberAsync(bookingNumber))
                .ReturnsAsync((Rental)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
                _sut.Handle(request, CancellationToken.None));

            exception.Message.Should().Be($"Rental not found for booking number {bookingNumber}");
            
            // Verify repository was called but update and cache were not
            _rentalRepository.Verify(x => x.GetByBookingNumberAsync(bookingNumber), Times.Once);
            _rentalRepository.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Never);
            _redisCacheService.Verify(x => x.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithReturnDateBeforePickup_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookingNumber = _fixture.Create<string>();
            var pickupDate = DateTime.Now.AddDays(1);
            var returnDate = DateTime.Now;

            var rental = new Rental
            {
                BookingNumber = bookingNumber,
                PickupDate = pickupDate,
                PickupMeterReading = 1000,
                CarCategory = CarCategory.Small
            };

            var command = new RegisterReturnCommand
            {
                BookingNumber = bookingNumber,
                ReturnDate = returnDate,
                ReturnMeterReading = 1500
            };

            _redisCacheService.Setup(x => x.GetCachedRentalAsync(bookingNumber))
                .ReturnsAsync(rental);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Return date cannot be before pickup date");
            
            // Verify update was not called
            _rentalRepository.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Never);
            _redisCacheService.Verify(x => x.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithLowerMeterReading_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookingNumber = _fixture.Create<string>();
            var rental = new Rental
            {
                BookingNumber = bookingNumber,
                PickupDate = DateTime.Now.AddDays(-1),
                PickupMeterReading = 2000,
                CarCategory = CarCategory.Small
            };

            var command = new RegisterReturnCommand
            {
                BookingNumber = bookingNumber,
                ReturnDate = DateTime.Now,
                ReturnMeterReading = 1500  // Less than pickup reading
            };

            _redisCacheService.Setup(x => x.GetCachedRentalAsync(bookingNumber))
                .ReturnsAsync(rental);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Return meter reading cannot be less than pickup meter reading");
            
            // Verify update was not called
            _rentalRepository.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Never);
            _redisCacheService.Verify(x => x.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithCachedRental_ShouldUseCacheAndNotCallRepository()
        {
            // Arrange
            var bookingNumber = _fixture.Create<string>();
            var rental = new Rental
            {
                BookingNumber = bookingNumber,
                PickupDate = new DateTime(2023, 10, 1),
                PickupMeterReading = 1000,
                CarCategory = CarCategory.Small
            };

            var command = new RegisterReturnCommand
            {
                BookingNumber = bookingNumber,
                ReturnDate = new DateTime(2023, 10, 6),
                ReturnMeterReading = 1500
            };

            _redisCacheService.Setup(r => r.GetCachedRentalAsync(bookingNumber))
                .ReturnsAsync(rental);
            _priceCalculationStrategyFactory
                .Setup(f => f.GetStrategy(CarCategory.Small))
                .Returns(new SmallCarPriceCalculation());

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ValidationErrors.Should().BeEmpty();
            _rentalRepository.Verify(r => r.GetByBookingNumberAsync(It.IsAny<string>()), Times.Never);
            _redisCacheService.Verify(r => r.GetCachedRentalAsync(bookingNumber), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterReturnCommand
            {
                BookingNumber = "TEST123",
                ReturnDate = DateTime.Now,
                ReturnMeterReading = 2000
            };

            _redisCacheService.Setup(x => x.GetCachedRentalAsync(request.BookingNumber))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(request, CancellationToken.None));

            exception.Message.Should().Be("Database error");
            
            // Verify that subsequent operations were not called
            _rentalRepository.Verify(x => x.GetByBookingNumberAsync(It.IsAny<string>()), Times.Never);
            _rentalRepository.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Never);
            _redisCacheService.Verify(x => x.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInvalidMeterReading_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var rental = new Rental
            {
                BookingNumber = "TEST123",
                PickupMeterReading = 2000
            };

            var request = new RegisterReturnCommand
            {
                BookingNumber = rental.BookingNumber,
                ReturnDate = DateTime.Now,
                ReturnMeterReading = 1000  // Less than pickup reading
            };

            _redisCacheService.Setup(x => x.GetCachedRentalAsync(request.BookingNumber))
                .ReturnsAsync(rental);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(request, CancellationToken.None));

            exception.Message.Should().Be("Return meter reading cannot be less than pickup meter reading");
            
            // Verify update was not called
            _rentalRepository.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInvalidReturnDate_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var rental = new Rental
            {
                BookingNumber = "TEST123",
                PickupDate = DateTime.Now
            };

            var request = new RegisterReturnCommand
            {
                BookingNumber = rental.BookingNumber,
                ReturnDate = rental.PickupDate.AddDays(-1),  // Before pickup date
                ReturnMeterReading = 3000
            };

            _redisCacheService.Setup(x => x.GetCachedRentalAsync(request.BookingNumber))
                .ReturnsAsync(rental);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(request, CancellationToken.None));

            exception.Message.Should().Be("Return date cannot be before pickup date");
            
            // Verify update was not called
            _rentalRepository.Verify(x => x.UpdateAsync(It.IsAny<Rental>()), Times.Never);
        }
    }
}