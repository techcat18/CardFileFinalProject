﻿using Core.Models;
using Core.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Abstractions.Interfaces
{
    public interface ITextMaterialRepository : IGenericRepository<TextMaterial>
    {
        Task<IEnumerable<TextMaterial>> GetByCategoryId(int categoryId);
        Task<IEnumerable<TextMaterial>> GetWithDetailsAsync();
        Task<IEnumerable<TextMaterial>> GetWithDetailsAsync(TextMaterialParameters parameters);
        Task<IEnumerable<TextMaterial>> GetByUser(User user, TextMaterialParameters parameters);
        Task<TextMaterial> GetByIdWithDetailsAsync(int id);
    }
}
