using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private DataContext context;

        public PhotoRepository(DataContext context)
        {
            this.context = context;
        }

        public PhotoRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }
        public async Task<Photo> GetPhotoByIdAsync(int id)
        {
            return await _context.Photos.IgnoreQueryFilters().Include(p=>p.AppUser).SingleOrDefaultAsync(x=>x.Id == id);
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync()
        {
             var query = _context.Photos.Where(x =>!x.IsApproved)
                .IgnoreQueryFilters().AsQueryable();
            return await query.ProjectTo<PhotoForApprovalDto>(_mapper.ConfigurationProvider).ToListAsync();
            
        }

        public void RomovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}