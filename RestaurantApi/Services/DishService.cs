using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;

namespace RestaurantAPI.Services
{
    public interface IDishService
    {
        int Create(int restaurantId, CreateDishDto dto);
        DishDto GetById(int restaurantId, int dishId);
        List<DishDto> GetAll(int restaurantId);
        
        void RemoveAll(int restaurantId);
    }

    public class DishService : IDishService
    {
        private readonly ResturantDbContext _dbContext;
        private readonly IMapper _mapper;

        public DishService(ResturantDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public int Create(int restaurantId, CreateDishDto dto)
        {
            var restaurant = GetRestaurantById(restaurantId);
            var dishEntity = _mapper.Map<Dish>(dto);

            dishEntity.RestaurantId = restaurantId;

            _dbContext.Dishes.Add(dishEntity);
            _dbContext.SaveChanges();
            return dishEntity.Id;
        }

        public DishDto GetById(int restaurantId, int dishId)
        {
           var restaurant = GetRestaurantById(restaurantId);

            var dish = _dbContext.Dishes.FirstOrDefault(r => r.Id==dishId);
            if (dish is null || dish.RestaurantId != restaurantId)
            {
                throw new NotFoundException("Dish not found");
            }
            
            var dishDto = _mapper.Map<DishDto>(dish);
            return dishDto;
        }

        public List<DishDto> GetAll(int restaurantId)
        {
          var restaurant =  _dbContext.Restaurants.Include(r => r.Dishes).FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant is null) throw new NotFoundException("Restaurant not found");

            var dishDtos = _mapper.Map<List<DishDto>>(restaurant.Dishes);
            return dishDtos;
        }

        public void RemoveAll (int restaurantId) 
        {
            var restaurant = GetRestaurantById(restaurantId);

            _dbContext.RemoveRange(restaurant.Dishes);
            _dbContext.SaveChanges();
        }
        
        private Restaurant GetRestaurantById (int restaurantId)
        {
            var restaurant = _dbContext.Restaurants
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == restaurantId);
            if (restaurant is null)
            {
                throw new NotFoundException("Restaruant not found");
            }
            return restaurant;
        }
    }
}
