using BookStore.Application.Services;
using BookStore.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers.Api;

[ApiController]
[Route("api/customers")]
public class CustomersApiController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersApiController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CustomerDto>>> Index(int page = 1, int pageSize = 10)
    {
        var result = await _customerService.GetPagedAsync(page, pageSize);
        var dto = new PagedResultDto<CustomerDto>
        {
            Items = result.Items.Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CreatedAt = c.CreatedAt
            }).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            HasPrevious = result.HasPrevious,
            HasNext = result.HasNext
        };

        return Ok(dto);
    }
}
