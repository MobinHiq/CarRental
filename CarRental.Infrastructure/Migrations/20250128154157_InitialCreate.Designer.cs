﻿// <auto-generated />
using System;
using CarRental.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarRental.Infrastructure.Migrations
{
    [DbContext(typeof(CarRentalDbContext))]
    [Migration("20250128154157_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CarRental.Domain.Entities.Rental", b =>
                {
                    b.Property<string>("BookingNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text");

                    b.Property<string>("CarCategory")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RegistrationNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CustomerSocialSecurityNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("PickupDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("PickupMeterReading")
                        .HasColumnType("integer");

                    b.HasKey("BookingNumber");

                    b.ToTable("Rentals");
                });
#pragma warning restore 612, 618
        }
    }
}
