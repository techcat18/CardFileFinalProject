﻿using AutoMapper;
using BLL.Profiles;
using Core.Models;
using DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardFileTests
{
    internal static class UnitTestHelper
    {
        public static DbContextOptions<AppDbContext> GetUnitTestDbOptions()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            using (var context = new AppDbContext(options))
            {
                SeedData(context);
            }

            return options;
        }

        public static IMapper CreateMapperProfile()
        {
            var textMaterialProfile = new TextMaterialProfile();
            var textMaterialCategoryProfile = new TextMaterialCategoryProfile();
            var userProfile = new UserProfile();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(textMaterialProfile);
                cfg.AddProfile(textMaterialCategoryProfile);
                cfg.AddProfile(userProfile);
            });

            return new Mapper(configuration);
        }

        public static void SeedData(AppDbContext context)
        {
            context.TextMaterials.AddRange(
                new TextMaterial { Id = 1, AuthorId = "1", ApprovalStatus = ApprovalStatus.Pending, Content = "firstContent", Title = "firstArticle", TextMaterialCategoryId = 1 },
                new TextMaterial { Id = 2, AuthorId = "2", ApprovalStatus = ApprovalStatus.Approved, Content = "secondContent", Title = "secondArticle", TextMaterialCategoryId = 1 },
                new TextMaterial { Id = 3, AuthorId = "2", ApprovalStatus = ApprovalStatus.Approved, Content = "thirdContent", Title = "thirdArticle", TextMaterialCategoryId = 2 });

            context.TextMaterialCategory.AddRange(
                new TextMaterialCategory { Id = 1, Title = "First one" },
                new TextMaterialCategory { Id = 2, Title = "Second one" });

            context.Users.AddRange(
                new User { Id = "1", UserName = "Tommy", Email = "tommy@gmail.com" },
                new User { Id = "2", UserName = "Johnny", Email = "johnny@gmail.com" },
                new User { Id = "3", UserName = "Bobby", Email = "bobby@gmail.com" });

            context.SaveChanges();
        }
    }
}
