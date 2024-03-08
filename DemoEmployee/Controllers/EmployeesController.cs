using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoEmployee.Data;
using OfficeOpenXml;
using EFCore.BulkExtensions;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace DemoEmployee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly DemoEmployeeDbContext _context;

        public EmployeesController(DemoEmployeeDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> AddEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;

                            var employees = new List<Employee>();

                            for (int row = 2; row <= rowCount; row++)
                            {
                                var employee = new Employee
                                {
                                    MaNV = worksheet.Cells[row, 1].Value.ToString(),
                                    TenNV = worksheet.Cells[row, 2].Value.ToString(),
                                    NgaySinh = DateTime.ParseExact(
                                        worksheet.Cells[row, 3].Value.ToString(),
                                        "M/d/yyyy",
                                        CultureInfo.InvariantCulture
                                    ),
                                };

                                employees.Add(employee);
                            }

                            await _context.BulkInsertAsync(employees);
                        }
                    }
                    scope.Complete();
                    return Ok("Successfully");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }


        [HttpGet("ListEmpoyee")]
        public IActionResult GetEmployees(int pageIndex = 1, int pageSize = 10)
        {
            var query = _context.Employees.OrderBy(e => e.Id);
            var totalRecords = _context.Employees.Count();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var today = DateTime.Today;
            var employees = query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new
                    {
                        MaNV = e.MaNV,
                        TenNV = e.TenNV,
                        NgaySinh = e.NgaySinh,
                        Age = today.Year - e.NgaySinh.Year
                    })
                    .ToList();

            return Ok(new
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Data = employees
            });
        }
    }
}
