﻿using Microsoft.EntityFrameworkCore;
using Azure.Core;
using Azure;
using System.Data;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using JoaliBackend.Models;


namespace Joali.Data
{
    public class EFCoreDbContext : DbContext
    {
        public EFCoreDbContext(DbContextOptions<EFCoreDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



        }
    }

}
