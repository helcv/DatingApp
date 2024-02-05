using Microsoft.AspNetCore.Mvc;

namespace API;

[ServiceFilter(typeof(LogUserActivity))]
[ApiController]
[Route("api/[controller]")]  
public class BaseApiController : ControllerBase
{

}
