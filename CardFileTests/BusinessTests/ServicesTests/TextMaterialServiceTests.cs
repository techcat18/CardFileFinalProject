﻿using BLL.Abstractions.cs.Interfaces;
using BLL.Services;
using BLL.Validation;
using Core.DTOs;
using Core.Models;
using Core.RequestFeatures;
using DAL.Abstractions.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardFileTests.BusinessTests.ServicesTests
{
    public class TextMaterialServiceTests
    {
        [Test]
        public async Task TextMaterialService_GetTextMaterials_ReturnsValidTextMaterials()
        {
            // Arrange
            var expected = GetTextMaterialDTOs;
            var textMaterialParams = new TextMaterialParameters();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockEmailService = new Mock<IEmailService>();

            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.GetWithDetailsAsync(textMaterialParams))
                .ReturnsAsync(GetTextMaterialEntities.AsEnumerable());

            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            var actual = await textMaterialService.GetTextMaterials(textMaterialParams);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task TextMaterialService_GetTextMaterialById_ReturnsSingleValue(int id)
        {
            // Arrange
            var expected = GetTextMaterialDTOs.FirstOrDefault(tm => tm.Id == id);
            var textMaterialParams = new TextMaterialParameters();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockEmailService = new Mock<IEmailService>();

            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.GetByIdWithDetailsAsync(id))
                .ReturnsAsync(GetTextMaterialEntities.FirstOrDefault(tm => tm.Id == id));

            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            var actual = await textMaterialService.GetTextMaterialById(id);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestCase(100)]
        [TestCase(212)]
        [TestCase(-323)]
        public async Task TextMaterialService_GetTextMaterialById_ThrowsExceptionIfIdIsInvalid(int id)
        {
            // Arrange
            var expected = GetTextMaterialDTOs.FirstOrDefault(tm => tm.Id == id);
            var textMaterialParams = new TextMaterialParameters();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockEmailService = new Mock<IEmailService>();

            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.GetByIdWithDetailsAsync(id))
                .ReturnsAsync(GetTextMaterialEntities.FirstOrDefault(tm => tm.Id == id));

            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            Func<Task> act = async () => await textMaterialService.GetTextMaterialById(id);

            // Assert
            await act.Should().ThrowAsync<CardFileException>();
        }

        [Test]
        public async Task TextMaterialService_CreateTextMaterial_CreatesTextMaterial()
        {
            // Arrange
            var textMaterial = new CreateTextMaterialDTO
            {
                Title = "fourthArticle",
                Content = "New one",
                AuthorId = "1",
                CategoryTitle = "First one"
            };
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.TextMaterialCategoryRepository.GetByTitleAsync(textMaterial.CategoryTitle))
                .ReturnsAsync(GetTextMaterialCategoryEntities.FirstOrDefault(x => x.Title == textMaterial.CategoryTitle));

            mockUnitOfWork
                .Setup(x => x.UserRepository.GetByIdAsync(textMaterial.AuthorId))
                .ReturnsAsync(GetUserEntities.FirstOrDefault(u => u.Id == textMaterial.AuthorId));

            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.CreateAsync(It.IsAny<TextMaterial>()));

            var mockEmailService = new Mock<IEmailService>();
            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            await textMaterialService.CreateTextMaterial(textMaterial);

            // Assert
            mockUnitOfWork.Verify(x => x.TextMaterialRepository
                .CreateAsync(It.Is<TextMaterial>(tm => tm.Title == textMaterial.Title && tm.Content == textMaterial.Content && tm.AuthorId == textMaterial.AuthorId)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestCase("invalid title")]
        [TestCase("wrong")]
        public async Task TextMaterialService_CreateTextMaterial_ThrowsExceptionIfInvalidCategoryTItle(string categoryTItle)
        {
            // Arrange
            var textMaterial = new CreateTextMaterialDTO
            {
                Title = "fourthArticle",
                Content = "New one",
                AuthorId = "1",
                CategoryTitle = categoryTItle
            };
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.TextMaterialCategoryRepository.GetByTitleAsync(textMaterial.CategoryTitle))
                .ReturnsAsync(GetTextMaterialCategoryEntities.FirstOrDefault(x => x.Title == textMaterial.CategoryTitle));

            mockUnitOfWork
                .Setup(x => x.UserRepository.GetByIdAsync(textMaterial.AuthorId))
                .ReturnsAsync(GetUserEntities.FirstOrDefault(u => u.Id == textMaterial.AuthorId));

            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.CreateAsync(It.IsAny<TextMaterial>()));

            var mockEmailService = new Mock<IEmailService>();
            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            Func<Task> act = async () => await textMaterialService.CreateTextMaterial(textMaterial);

            // Assert
            await act.Should().ThrowAsync<CardFileException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task TextMaterialService_DeleteTextMaterial_DeletesTextMaterial(int id)
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockEmailService = new Mock<IEmailService>();
            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.DeleteById(It.IsAny<int>()));
            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.GetByIdAsync(id))
                .ReturnsAsync(GetTextMaterialEntities.FirstOrDefault(tm => tm.Id == id));
            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            await textMaterialService.DeleteTextMaterial(id);

            // Assert
            mockUnitOfWork.Verify(x => x.TextMaterialRepository.DeleteById(id), Times.Once());
            mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [TestCase(-23)]
        [TestCase(10000)]
        [TestCase(0)]
        public async Task TextMaterialService_DeleteTextMaterial_ThrowsExceptionIfIdIsInvalidl(int id)
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockEmailService = new Mock<IEmailService>();
            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.DeleteById(It.IsAny<int>()));
            mockUnitOfWork
                .Setup(x => x.TextMaterialRepository.GetByIdAsync(id))
                .ReturnsAsync(GetTextMaterialEntities.FirstOrDefault(tm => tm.Id == id));
            var textMaterialService = new TextMaterialService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile(), mockEmailService.Object);

            // Act
            Func<Task> act = async () => await textMaterialService.DeleteTextMaterial(id);

            // Assert
            await act.Should().ThrowAsync<CardFileException>();
        }

        #region TestData

        public static List<User> GetUserEntities =
            new List<User>()
            {
                new User { Id = "1", UserName = "Tommy", Email = "tommy@gmail.com" },
                new User { Id = "2", UserName = "Johnny", Email = "johnny@gmail.com" }
            };

        private static List<TextMaterialCategory> GetTextMaterialCategoryEntities =
            new List<TextMaterialCategory>
            {
                new TextMaterialCategory { Id = 1, Title = "First one" },
                new TextMaterialCategory { Id = 2, Title = "Second one" }
            };

        public List<TextMaterial> GetTextMaterialEntities =
            new List<TextMaterial>()
            {
                new TextMaterial { Id = 1, Author = GetUserEntities[0], AuthorId = "1", TextMaterialCategory = GetTextMaterialCategoryEntities[0], ApprovalStatus = ApprovalStatus.Pending, Content = "firstContent", Title = "firstArticle", TextMaterialCategoryId = 1, DatePublished = new DateTime(2000,11,23) },
                new TextMaterial { Id = 2, Author = GetUserEntities[1], AuthorId = "2", TextMaterialCategory = GetTextMaterialCategoryEntities[0], ApprovalStatus = ApprovalStatus.Approved, Content = "secondContent", Title = "secondArticle", TextMaterialCategoryId = 1, DatePublished = new DateTime(2010,8,17) },
                new TextMaterial { Id = 3, Author = GetUserEntities[1], AuthorId = "2", TextMaterialCategory = GetTextMaterialCategoryEntities[1], ApprovalStatus = ApprovalStatus.Approved, Content = "thirdContent", Title = "thirdArticle", TextMaterialCategoryId = 2, DatePublished = new DateTime(2013,1,18) }
            };

        private List<TextMaterialDTO> GetTextMaterialDTOs =
            new List<TextMaterialDTO>
            {
                new TextMaterialDTO { Id = 1, AuthorId = "1", UserName = "Tommy", ApprovalStatusId = 0, Content = "firstContent", Title = "firstArticle", CategoryTitle = "First one", DatePublished = new DateTime(2000,11,23) },
                new TextMaterialDTO { Id = 2, AuthorId = "2", UserName = "Johnny", ApprovalStatusId = 1, Content = "secondContent", Title = "secondArticle", CategoryTitle = "First one", DatePublished = new DateTime(2010,8,17) },
                new TextMaterialDTO { Id = 3, AuthorId = "2", UserName = "Johnny", ApprovalStatusId = 1, Content = "thirdContent", Title = "thirdArticle", CategoryTitle = "Second one", DatePublished = new DateTime(2013,1,18) }
            };

        #endregion
    }
}