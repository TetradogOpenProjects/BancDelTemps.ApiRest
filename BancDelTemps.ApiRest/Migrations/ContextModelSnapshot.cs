﻿// <auto-generated />
using System;
using BancDelTemps.ApiRest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BancDelTemps.ApiRest.Migrations
{
    [DbContext(typeof(Context))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.7");

            modelBuilder.Entity("BancDelTemps.ApiRest.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime?>("EndHolidays")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("IdExterno")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("JoinDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("StartHolidays")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int?>("ValidatorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ValidatorId");

                    b.HasIndex(new[] { "Email" }, "Email_uniqueContraint")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BancDelTemps.ApiRest.Models.User", b =>
                {
                    b.HasOne("BancDelTemps.ApiRest.Models.User", "Validator")
                        .WithMany("Validated")
                        .HasForeignKey("ValidatorId");

                    b.Navigation("Validator");
                });

            modelBuilder.Entity("BancDelTemps.ApiRest.Models.User", b =>
                {
                    b.Navigation("Validated");
                });
#pragma warning restore 612, 618
        }
    }
}
