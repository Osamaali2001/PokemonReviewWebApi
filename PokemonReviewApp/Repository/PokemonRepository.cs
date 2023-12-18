using PokemonReviewApp.Data;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using System.Diagnostics.Metrics;

namespace PokemonReviewApp.Repository
{
    public class PokemonRepository : IPokemonRepository
    {
        private readonly DataContext _context;

        public PokemonRepository(DataContext context) 
        {
            _context = context;
        }

        public bool CreatePokemon(int ownerId, int categoryId, Pokemon pokemon)
        {
            var Owner = _context.Owners.FirstOrDefault(a => a.Id == ownerId);
            var category = _context.Categories.FirstOrDefault(a => a.Id == categoryId);

            var pokemonOwner = new PokemonOwner()
            {
                Owner = Owner,
                Pokemon = pokemon
            };

            _context.Add(pokemonOwner);

            var pokemonCategory = new PokemonCategory()
            {
                Category = category,
                Pokemon = pokemon
            };

            _context.Add(pokemonCategory);

            _context.Add(pokemon);

            return Save();
        }

        public bool DeletePokemon(Pokemon pokemon)
        {
            _context.Remove(pokemon);
            return Save();
        }

        public Pokemon GetPokemon(int id)
        {
            return _context.Pokemon.FirstOrDefault(p => p.Id == id);
        }

        public Pokemon GetPokemon(string name)
        {
            return _context.Pokemon.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
        }

        public decimal GetPokemonRating(int id)
        {
            decimal rating = 0;
            var reviews = _context.Reviews.Where(r => r.Pokemon.Id == id);
            if(reviews.Any()) 
            {
                rating = (decimal)reviews.Sum(r => r.Rating) / reviews.Count();
            }
            return rating;
        }

        public ICollection<Pokemon> GetPokemons()
        {
            return _context.Pokemon.OrderBy(p => p.Id).ToList();
        }

        public bool PokemonExists(int id)
        {
            return _context.Pokemon.Any(p => p.Id == id);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdatePokemon(int ownerId, int categoryId, Pokemon pokemon)
        {
            _context.Update(pokemon);
            return Save();
        }
    }
}
