using EatonDeliveryCheckpoint.Dtos;
using EatonDeliveryCheckpoint.Interfaces;
using EatonDeliveryCheckpoint.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EatonDeliveryCheckpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        public DeliveryController(DeliveryService service)
        {
            _service = service;
        }

        private readonly DeliveryService _service;

        // GET api/<DeliveryController>/cargo
        [HttpGet("cargo")]
        public IActionResult GetCargo()
        {
            CargoResultDto dto = (CargoResultDto)_service.GetCargo();
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // POST api/<DeliveryController>
        [HttpPost("upload")]
        public IActionResult Post([FromBody] dynamic value)
        {
            ResultDto dto = _service.Post(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // GET api/<DeliveryController>/cargo
        [HttpPost("work")]
        public IActionResult PostWork()
        {
            return Ok();
        }
    }
}
