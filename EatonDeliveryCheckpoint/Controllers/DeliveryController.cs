using EatonDeliveryCheckpoint.Dtos;
using EatonDeliveryCheckpoint.Services;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult PostToUploadFile([FromBody] dynamic value)
        {
            ResultDto dto = _service.PostToUploadFile(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // POST api/<DeliveryController>
        [HttpPost("start")]
        public IActionResult PostToStart([FromBody] dynamic value)
        {
            ResultDto dto = _service.PostToStart(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // POST api/<DeliveryController>
        [HttpPost("terminal")]
        public IActionResult PostFromTerminal([FromBody] dynamic value)
        {
            ResultDto dto = _service.PostFromTerminal(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }
    }
}
