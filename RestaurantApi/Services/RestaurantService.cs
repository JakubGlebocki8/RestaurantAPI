using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        RestaurantDto GetById(int id);
        PagedResult<RestaurantDto> GetAll(RestaurantQuery query);
        int Create(CreateRestaurantDto dto);
        void Delete(int id);
        void Update (int id, UpdateRestaurantDto dto);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly ResturantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public RestaurantService(ResturantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger, IAuthorizationService authorizationService, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }

        public void Update(int id, UpdateRestaurantDto dto)
        {


            var existingRestaurant = _dbContext.Restaurants.FirstOrDefault(x => x.Id == id);

            if (existingRestaurant is null)
            {
                throw new NotFoundException("Restaruant not found");
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, existingRestaurant, new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }
            existingRestaurant.Name = dto.Name;
            existingRestaurant.Description = dto.Description;
            existingRestaurant.HasDelivery = dto.HasDelivery;

            _dbContext.SaveChanges();


        }

        public void Delete(int id)
        {
            _logger.LogError($"Restaurant with id: {id} DELETE action invoked ");
            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == id);

            if (restaurant is null) throw new NotFoundException("Restaurant Not found");
            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurant, new ResourceOperationRequirement(ResourceOperation.Delete)).Result;
            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            _dbContext.Remove(restaurant);
            _dbContext.SaveChanges();

        }
        public RestaurantDto GetById(int id)
        {

            var restaurant = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == id);

            if (restaurant is null) throw new NotFoundException("Restaurant not Found");
            var result = _mapper.Map<RestaurantDto>(restaurant);
            return result;

        }
        public PagedResult<RestaurantDto> GetAll(RestaurantQuery query)
        {
            var baseQuery = _dbContext.Restaurants
               .Include(r => r.Address)
               .Include(r => r.Dishes)
               .Where(r => query.SearchPhrase == null || (r.Name.ToLower().Contains(query.SearchPhrase.ToLower())
               || r.Description.ToLower().Contains(query.SearchPhrase.ToLower())));

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                var coulmnsSelector = new Dictionary<string, Expression<Func<Restaurant, Object>>>
                {
                    {nameof(Restaurant.Name),r => r.Name},
                    {nameof(Restaurant.Description),r => r.Description},
                    {nameof(Restaurant.Category),r => r.Category},
                };
                var selectedColumn = coulmnsSelector[query.SortBy];

              baseQuery = query.SortDirection == SortDirection.ASC 
                    ? baseQuery.OrderBy(selectedColumn) 
                    : baseQuery.OrderByDescending(selectedColumn); 
            }
            var restaurant = baseQuery
               .Skip(query.PageSize* (query.PageNumber - 1))
               .Take(query.PageSize)
               .ToList();
         var totalItemsCount =   baseQuery.Count();

            var restaurantDtos = _mapper.Map<List<RestaurantDto>>(restaurant);
            var result = new PagedResult<RestaurantDto>(restaurantDtos, totalItemsCount, query.PageSize,query.PageNumber);
            return result;
      
        }
        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);
            restaurant.CreatedById = _userContextService.GetUserId;
            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();
            return restaurant.Id;
        }
    }
}
