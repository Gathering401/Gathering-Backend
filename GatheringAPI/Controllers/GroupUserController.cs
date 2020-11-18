using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Models;
using GatheringAPI.Services;

namespace GatheringAPI.Controllers
{
    [Route("api/Group")]
    [ApiController]
    [Authorize]
    public class GroupUserController : ControllerBase
    {
        
    }
}
