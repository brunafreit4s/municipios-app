using Microsoft.AspNetCore.Mvc;
using municipios_api.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore;
using municipios_api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Filters;

namespace municipios_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MunicipiosController : Controller
    {
        private readonly string _dataBaseName = "MunicipiosDataBase";
        private readonly ILogger<MunicipiosController> _logger;
        private readonly string _url = "https://servicodados.ibge.gov.br/api/v1/localidades/estados/MG/municipios";

        public MunicipiosController(ILogger<MunicipiosController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Persiste os dados da API de IBGE no banco de dados.
        /// </summary>
        /// <returns>Não há retorno.</returns>
        /// <response code="204">Dados persistidos com sucesso (sem conteúdo de resposta).</response>
        /// <response code="404">Nenhum dado retornado da API de IBGE.</response>
        /// <response code="400">Erro na requisição.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Post()
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

                        if (municipios != null && municipios.Count > 0)
                        {
                            var options = new DbContextOptionsBuilder<ApplicationContext>()
                                .UseInMemoryDatabase(databaseName: _dataBaseName)
                                .Options;

                            using (var context = new ApplicationContext(options))
                            {
                                // Adiciona um range de municípios
                                context.Municipios.AddRangeAsync(municipios);
                                context.SaveChangesAsync();                                
                            }

                            _logger.LogInformation($"Sucesso ao buscar persistir os dados de municípios, total de: {municipios.Count} municípios persistidos no banco de dados.");
                            return NoContent();
                        }

                        _logger.LogInformation("Nenhum município foi encontrado para persistir na base de dados!");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError($"Erro: {response.StatusCode} - POST() in MunicipiosController");
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao persistir municípios: {ex.Message} - POST() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }

        /// <summary>
        /// Obtém todos municípios persistidos no banco de dados.
        /// </summary>
        /// <returns>Obtém lista de municípios persistidos no banco de dados.</returns>
        /// <response code="200">Dados persistidos no banco de dados.</response>
        /// <response code="404">Nenhum dado encontrado no banco de dados.</response>
        /// <response code="400">Erro na requisição.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Get()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var options = new DbContextOptionsBuilder<ApplicationContext>()
                                .UseInMemoryDatabase(databaseName: _dataBaseName)
                                .Options;

                    using (var context = new ApplicationContext(options))
                    {
                        // Lista dados da tabela de municípios
                        var municipios = context.Municipios.ToList();

                        if (municipios != null && municipios.Count > 0)                        
                        {
                            _logger.LogInformation($"Sucesso ao buscar municípios, total de: {municipios.Count} municípios encontrados.");
                            return Ok(municipios);
                        }
                    }

                    _logger.LogInformation("Nenhum município encontrado!");
                    return NotFound();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao buscar municípios: {ex.Message} - GET() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }

        /// <summary>
        /// Obtém todos municípios persistidos no banco de dados.
        /// </summary>
        /// <param name="id">Número de identificação do município.</param>
        /// <returns>Obtém lista de municípios persistidos no banco de dados.</returns>
        /// <response code="200">Dados persistidos no banco de dados.</response>
        /// <response code="404">Nenhum dado encontrado no banco de dados.</response>
        /// <response code="400">Erro na requisição.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Get(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var options = new DbContextOptionsBuilder<ApplicationContext>()
                                .UseInMemoryDatabase(databaseName: _dataBaseName)
                                .Options;

                    using (var context = new ApplicationContext(options))
                    {
                        // Adiciona um range de municípios
                        var municipio = context.Municipios.Find(id);

                        if (municipio != null)
                        {
                            _logger.LogInformation($"Sucesso ao buscar município: {municipio.Nome}.");
                            return Ok(municipio);
                        }
                    }

                    _logger.LogInformation($"Nenhum município encontrado!");
                    return NotFound();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao buscar município: {ex.Message} - GET() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }
    }
}
