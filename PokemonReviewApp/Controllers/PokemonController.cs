using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;
using PokemonReviewApp.Repository;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IOwnerRepository _ownerRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;

        public PokemonController(IPokemonRepository pokemonRepository,
            IOwnerRepository ownerRepository,
            IReviewRepository reviewRepository,
            IMapper mapper)
        {
            _pokemonRepository = pokemonRepository;
            _ownerRepository = ownerRepository;
            _reviewRepository = reviewRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public IActionResult GetPokemons()
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(_pokemonRepository.GetPokemons());

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(pokemons);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemon(int id)
        {
            if (!_pokemonRepository.PokemonExists(id))
                return NotFound();
            var pokemon = _mapper.Map<PokemonDto>(_pokemonRepository.GetPokemon(id));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemon);
        }

        [HttpGet("{id}/rating")]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public IActionResult GetPokemonRating(int id)
        {
            if (!_pokemonRepository.PokemonExists(id))
                return NotFound();
            var rating = _pokemonRepository.GetPokemonRating(id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(rating);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreatePokemon([FromQuery] int ownerId, [FromQuery] int categoryId, [FromBody] PokemonDto pokemonCreated)
        {
            if (pokemonCreated == null)
                return BadRequest(ModelState);

            var pokemon = _pokemonRepository.GetPokemons().FirstOrDefault(c => c.Name.Trim().ToUpper() == pokemonCreated.Name.Trim().ToUpper());

            if (pokemon != null)
            {
                ModelState.AddModelError("", "Pokemon already exists");
                return StatusCode(422, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var pokemonMap = _mapper.Map<Pokemon>(pokemonCreated);

            if (!_pokemonRepository.CreatePokemon(ownerId, categoryId, pokemonMap))
            {
                ModelState.AddModelError("", "Somthing went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully created");
        }

        [HttpPut("{pokemonId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePokemon(int pokemonId,
            [FromQuery] int ownerId,
            [FromQuery] int categoryId,
            [FromBody] PokemonDto pokemonUpdated)
        {
            if (pokemonUpdated == null)
                return BadRequest(ModelState);

            if (pokemonId != pokemonUpdated.Id)
                return BadRequest(ModelState);

            if (!_pokemonRepository.PokemonExists(pokemonId))
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest();

            var pokemonMap = _mapper.Map<Pokemon>(pokemonUpdated);

            if (!_pokemonRepository.UpdatePokemon(ownerId,categoryId,pokemonMap))
            {
                ModelState.AddModelError("", "Somthing went wrong while updating pokemon");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{pokemonId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeletePokemon(int pokemonId)
        {
            if (!_pokemonRepository.PokemonExists(pokemonId))
                return NotFound();

            var pokemonToDelete = _pokemonRepository.GetPokemon(pokemonId);
            var reviewsToDelete = _reviewRepository.GetReviewsOfAPokemon(pokemonId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
                ModelState.AddModelError("", "Somthing went wrong deleting reviews of a pokemon");

            if (!_pokemonRepository.DeletePokemon(pokemonToDelete))
            {
                ModelState.AddModelError("", "Somthing went wrong deleting pokemon");
            }

            return NoContent();
        }
    }
}
