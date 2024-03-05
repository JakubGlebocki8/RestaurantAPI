using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Security.Claims;

namespace RestaurantAPI.Controllers
{
    [Route("api/restaurant")]
    [ApiController]
    [Authorize]
    public class RestaurantControler : ControllerBase
    {
        private readonly RestaurantService _restaurantservice;

        public RestaurantControler(IRestaurantService restaurantService)
        {
            _restaurantservice = (RestaurantService)restaurantService;
        } 
        [HttpDelete("{id}")]
        public ActionResult Delete([FromRoute] int id)
        {
             _restaurantservice.Delete(id);

           

            return NoContent();
        }
        [HttpPut("{id}")]
        public ActionResult Update ([FromRoute] int id, [FromBody] UpdateRestaurantDto dto )
        {
            
           _restaurantservice.Update(id, dto);
           
            return Ok();

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult CreateResturant([FromBody]CreateRestaurantDto dto)
        {
           
          var id =  _restaurantservice.Create(dto);
            return Created($"/api/restaurant/{id}",null);
        }

        [HttpGet]
       // [Authorize(Policy = "CreatedAtleast2Restaurant")]
        public ActionResult<IEnumerable<RestaurantDto>> GetAll([FromQuery]RestaurantQuery query)
        {
           var restaruantDtos = _restaurantservice.GetAll(query);

            return Ok(restaruantDtos);
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<RestaurantDto> Get([FromRoute] int id) 
        {
            var restaurant = _restaurantservice.GetById(id);
            
            return Ok(restaurant);
        }
    }
}
