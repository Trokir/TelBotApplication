﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TelBotApplication.DAL;

#nullable disable

namespace TelBotApplication.DAL.Migrations
{
    [DbContext(typeof(TelBotApplicationDbContext))]
    [Migration("20220622165646_Chaq11")]
    partial class Chaq11
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("TelBotApplication.Domain.Models.Admin", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Anchors.Anchor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnchorAction")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnchorCallBackType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Filter")
                        .HasColumnType("INTEGER");

                    b.Property<long>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GroupId1")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UntilMinutes")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GroupId1");

                    b.ToTable("Anchors");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Anchors.AnchorCallback", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnchorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ButtonCondition")
                        .HasColumnType("TEXT");

                    b.Property<string>("ButtonText")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AnchorId")
                        .IsUnique();

                    b.ToTable("AnchorCallbacks");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.BotCaller", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Caption")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(24)
                        .HasColumnType("TEXT");

                    b.Property<string>("Link")
                        .HasColumnType("TEXT");

                    b.Property<int>("TypeOfreaction")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("BotCallers");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.MessageLogger", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddedDate")
                        .HasColumnType("TEXT");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FullName")
                        .HasColumnType("TEXT");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .HasColumnType("TEXT");

                    b.Property<int>("TypeOfMessageLog")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.ToTable("MessageLoggers");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.TextFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Comment")
                        .HasColumnType("TEXT");

                    b.Property<int>("Filter")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TextFilters");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.VenueCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .HasColumnType("TEXT");

                    b.Property<string>("Command")
                        .HasColumnType("TEXT");

                    b.Property<float>("Latitude")
                        .HasColumnType("REAL");

                    b.Property<float>("Longitude")
                        .HasColumnType("REAL");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("VenueCommands");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Admin", b =>
                {
                    b.HasOne("TelBotApplication.Domain.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Anchors.Anchor", b =>
                {
                    b.HasOne("TelBotApplication.Domain.Models.Group", "Group")
                        .WithMany("Anchors")
                        .HasForeignKey("GroupId1");

                    b.Navigation("Group");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Anchors.AnchorCallback", b =>
                {
                    b.HasOne("TelBotApplication.Domain.Models.Anchors.Anchor", "Anchor")
                        .WithOne("AnchorCallback")
                        .HasForeignKey("TelBotApplication.Domain.Models.Anchors.AnchorCallback", "AnchorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Anchor");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.MessageLogger", b =>
                {
                    b.HasOne("TelBotApplication.Domain.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Anchors.Anchor", b =>
                {
                    b.Navigation("AnchorCallback");
                });

            modelBuilder.Entity("TelBotApplication.Domain.Models.Group", b =>
                {
                    b.Navigation("Anchors");
                });
#pragma warning restore 612, 618
        }
    }
}