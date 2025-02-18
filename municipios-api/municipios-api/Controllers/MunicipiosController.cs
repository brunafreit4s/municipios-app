using Microsoft.AspNetCore.Mvc;
using municipios_api.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.InMemory;

namespace municipios_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MunicipiosController : Controller
    {
        private readonly ILogger<MunicipiosController> _logger;
        private readonly string _url = "https://servicodados.ibge.gov.br/api/v1/localidades/estados/MG/municipios";

        public MunicipiosController(ILogger<MunicipiosController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    List<Municipio> municipios = new List<Municipio>();
                    HttpResponseMessage response = await client.GetAsync(_url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Desserializa a resposta para uma lista de objetos anônimos
                        municipios = JsonSerializer.Deserialize<List<Municipio>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if(municipios.Count > 0)
                        {
                            _logger.LogInformation($"Sucesso em buscar municípios, total de: {municipios.Count} municípios encontrados.");
                            return Ok(municipios);
                        }

                        _logger.LogInformation("Nenhum município encontrado!");
                        return NotFound();
                    }
                    else
                    {                        
                        _logger.LogError($"Erro: {response.StatusCode} - GET() in MunicipiosController");
                        return NoContent();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao buscar municípios: {ex.Message} - GET() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }
    }
}
