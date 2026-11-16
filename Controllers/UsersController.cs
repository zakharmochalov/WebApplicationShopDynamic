using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationShop.Data;
using WebApplicationShop.Dto;

namespace WebApplicationShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        // по каким столбцам таблицы можно сортировать (свойства UserDto)
        private static readonly string[] SortFields = { "Id", "Name", "Email", "RoleName" };

        public UsersController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? role, string? sort, bool desc = false)
        {
            var query = _context.Users.AsQueryable();

            // динамический Where с параметром @0
            if (!string.IsNullOrEmpty(role))
                query = query.Where("Role != null && Role.Name == @0", role);

            if (sort == null || !SortFields.Contains(sort))
                sort = "Id";

            // ProjectTo (AutoMapper) + динамический OrderBy по свойству UserDto, всё выполняется в одном SQL-запросе
            List<UserDto> users = await query.ProjectTo<UserDto>(_mapper.ConfigurationProvider).OrderBy(desc ? sort + " desc" : sort).ToListAsync();

            ViewBag.Role = role;
            ViewBag.Roles = await _context.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Sort = sort;
            ViewBag.Desc = desc;
            return View(users);
        }
    }
}