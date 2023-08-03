﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ReunionServer;

#nullable disable

namespace ReunionServer.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230802030404_R001")]
    partial class R001
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_general_ci")
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4");

            modelBuilder.Entity("ReunionServer.Models.CampaignSelectorMark", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("id");

                    b.Property<int>("Bad")
                        .HasMaxLength(255)
                        .HasColumnType("int")
                        .HasColumnName("bad");

                    b.Property<int>("Good")
                        .HasMaxLength(255)
                        .HasColumnType("int")
                        .HasColumnName("good");

                    b.Property<string>("Usname")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("USName");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.ToTable("CampaignSelectorMark", (string)null);

                    MySqlEntityTypeBuilderExtensions.UseCollation(b, "utf8mb4_0900_ai_ci");
                });
#pragma warning restore 612, 618
        }
    }
}
