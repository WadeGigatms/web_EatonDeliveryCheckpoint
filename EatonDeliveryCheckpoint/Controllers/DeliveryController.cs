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

        // GET api/<DeliveryController>/dnlist
        [HttpGet("dnlist")]
        public IActionResult GetDnList()
        {
            DeliveryCargoResultDto dto = (DeliveryCargoResultDto)_service.GetDnList();
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // GET api/<DeliveryController>/search
        [HttpGet("search")]
        public IActionResult GetReview([FromQuery] string no)
        {
            DeliveryCargoResultDto dto = (DeliveryCargoResultDto)_service.GetSearch(no);
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
        [HttpPost("finish")]
        public IActionResult PostToFinish([FromBody] dynamic value)
        {
            ResultDto dto = _service.PostToFinish(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // POST api/<DeliveryController>
        [HttpPost("terminal")]
        public IActionResult PostFromEpcServer([FromBody] dynamic value)
        {
            ResultDto dto = _service.PostFromEpcServer(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // POST api/<DeliveryController>
        [HttpPost("dismissalert")]
        public IActionResult PostToDismissAlert([FromBody] dynamic value)
        {
            ResultDto dto = (ResultDto)_service.PostToDismissAlert(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }

        // POST api/<DeliveryController>
        [HttpPost("quit")]
        public IActionResult PostToQuit([FromBody] dynamic value)
        {
            ResultDto dto = (ResultDto)_service.PostToQuit(value);
            return dto.Result == true ? Ok(dto) : BadRequest(dto);
        }
    }
}
