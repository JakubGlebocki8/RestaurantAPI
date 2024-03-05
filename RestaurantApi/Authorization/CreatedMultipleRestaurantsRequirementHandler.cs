using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class CreatedMultipleRestaurantsRequirementHandler : AuthorizationHandler<CreatedMultipleRestaurantsRequirement>
    {
        private readonly ResturantDbContext _dbContext;

        public CreatedMultipleRestaurantsRequirementHandler(ResturantDbContext dbContext)
        {
           _dbContext = dbContext;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CreatedMultipleRestaurantsRequirement requirement)
        {
            var userId = int.Parse(context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier ).Value);
         var createdRestaurantsCount = _dbContext.Restaurants.Count(c => c.CreatedById == userId);
            if( createdRestaurantsCount >= requirement.MinimumRestaurantCreated)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
