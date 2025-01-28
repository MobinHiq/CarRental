using Moq;
using CarRental.Application.Handlers;
using CarRental.Application.Commands;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CarRental.Domain.Entities;
using CarRental.Domain.Enums;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Interfaces;
using AutoFixture;
using FluentValidation;

namespace CarRental.Tests.UnitTests
{
    public class RegisterPickupCommandHandlerTests : IDisposable
    {
        private readonly Mock<IRentalRepository> _rentalRepository;
        private readonly Mock<IRedisCacheService> _redisCacheService;
        private readonly Mock<ILogger<RegisterPickupCommandHandler>> _logger;
        private readonly RegisterPickupCommandHandler _sut;
        private readonly Fixture _fixture;

        public RegisterPickupCommandHandlerTests()
        {
            _fixture = new Fixture();
            _rentalRepository = new Mock<IRentalRepository>();
            _redisCacheService = new Mock<IRedisCacheService>();
            _logger = new Mock<ILogger<RegisterPickupCommandHandler>>();

            _sut = new RegisterPickupCommandHandler(
                _rentalRepository.Object,
                _redisCacheService.Object,
                _logger.Object
            );
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateRentalAndReturnSuccess()
        {
            // Arrange
            var rental = new Rental
            {
                BookingNumber = _fixture.Create<string>(),
                CustomerSocialSecurityNumber = "123456789",
                RegistrationNumber = "ABC123",
                PickupDate = DateTime.Now,
                PickupMeterReading = 1000,
                CarCategory = CarCategory.Small
            };

            var command = new RegisterPickupCommand
            {
                Rental = rental
            };

            _rentalRepository.Setup(r => r.CreateAsync(rental))
                .ReturnsAsync(rental);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.BookingNumber.Should().Be(rental.BookingNumber);
            result.Data.CustomerSocialSecurityNumber.Should().Be(rental.CustomerSocialSecurityNumber);
            result.Data.RegistrationNumber.Should().Be(rental.RegistrationNumber);
            result.Data.PickupDate.Should().Be(rental.PickupDate);
            result.Data.PickupMeterReading.Should().Be(rental.PickupMeterReading);

            _rentalRepository.Verify(r => r.CreateAsync(rental), Times.Once);
            _redisCacheService.Verify(r => r.CacheRentalAsync(rental.BookingNumber, rental), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrowsException_ShouldThrowException()
        {
            // Arrange
            var rental = new Rental
            {
                CustomerSocialSecurityNumber = "123456789",
                RegistrationNumber = "ABC123",
                PickupDate = DateTime.Now,
                PickupMeterReading = 1000
            };

            var command = new RegisterPickupCommand
            {
                Rental = rental
            };

            _rentalRepository.Setup(r => r.CreateAsync(rental))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(command, CancellationToken.None));

            _rentalRepository.Verify(r => r.CreateAsync(rental), Times.Once);
            _redisCacheService.Verify(r => r.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithInvalidOperation_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var rental = new Rental
            {
                CustomerSocialSecurityNumber = "123456789",
                RegistrationNumber = "ABC123",
                PickupDate = DateTime.Now,
                PickupMeterReading = 1000
            };

            var command = new RegisterPickupCommand
            {
                Rental = rental
            };

            var errorMessage = "Invalid operation";
            _rentalRepository.Setup(r => r.CreateAsync(rental))
                .ThrowsAsync(new InvalidOperationException(errorMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(command, CancellationToken.None));

            exception.Message.Should().Be(errorMessage);
            _rentalRepository.Verify(r => r.CreateAsync(rental), Times.Once);
            _redisCacheService.Verify(r => r.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNullRental_ShouldThrowValidationException()
        {
            // Arrange
            var command = new RegisterPickupCommand
            {
                Rental = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _sut.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Rental cannot be null");
            _rentalRepository.Verify(r => r.CreateAsync(It.IsAny<Rental>()), Times.Never);
            _redisCacheService.Verify(r => r.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Handle_WithInvalidBookingNumber_ShouldThrowInvalidOperationException(string bookingNumber)
        {
            // Arrange
            var rental = new Rental
            {
                BookingNumber = bookingNumber,
                CustomerSocialSecurityNumber = "123456789",
                RegistrationNumber = "ABC123",
                PickupDate = DateTime.Now,
                PickupMeterReading = 1000
            };

            var command = new RegisterPickupCommand
            {
                Rental = rental
            };

            _rentalRepository.Setup(r => r.CreateAsync(rental))
                .ReturnsAsync((Rental)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _sut.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("Failed to create rental");
            _rentalRepository.Verify(r => r.CreateAsync(rental), Times.Once);
            _redisCacheService.Verify(r => r.CacheRentalAsync(It.IsAny<string>(), It.IsAny<Rental>()), Times.Never);
        }
    }
} 