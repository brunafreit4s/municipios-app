using Microsoft.AspNetCore.Mvc;
using municipios_api.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using municipios_api.Data;

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
        public IActionResult GetById(int id)
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
                        // Obtém um município por ID
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
                    _logger.LogError($"Erro ao buscar município: {ex.Message} - GETBYID() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }

        /// <summary>
        /// Atualiza os dados do município.
        /// </summary>
        /// <param name="id">Número de identificação do município.</param>
        /// <returns>Dados atualizados com sucesso.</returns>
        /// <response code="200">Dados atualizados no banco de dados.</response>
        /// <response code="404">Nenhum dado encontrado no banco de dados.</response>
        /// <response code="400">Erro na requisição.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Put(int id, Municipio municipio)
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
                        // Busca um município por id no banco de dados
                        var municipioExistente = context.Municipios.Find(id);

                        if (municipioExistente == null)
                        {
                            _logger.LogInformation($"Nenhum município encontrado com o ID: {id}.");
                            return NotFound();
                        }

                        // Se o município for encontrado, efetua a atualização
                        context.Entry(municipioExistente).CurrentValues.SetValues(municipio);
                        context.SaveChanges();

                        _logger.LogInformation($"Sucesso ao atualizar município: {municipioExistente.Nome}.");
                        return Ok(municipioExistente);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao atualizar município: {ex.Message} - PUT() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }


        /// <summary>
        /// Deleta todos municípios persistidos no banco de dados.
        /// </summary>
        /// <returns>Não há retorno.</returns>
        /// <response code="204">Todos os municípios foram deletados com sucesso do banco de dados.</response>
        /// <response code="404">Nenhum dado encontrado no banco de dados.</response>
        /// <response code="400">Erro na requisição.</response>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Delete()
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
                        // Busca todos os municípios persistidos no banco de dados
                        var municipios = context.Municipios.ToList();

                        if (!municipios.Any())
                        {
                            _logger.LogInformation("Nenhum município encontrado para deletar.");
                            return NotFound();
                        }

                        // Se existir algum município na base, efetua a deleção
                        context.Municipios.RemoveRange(municipios);
                        context.SaveChanges();                        

                        _logger.LogInformation($"Sucesso ao deletar todos municípios.");
                        return NoContent();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao deletar todos municípios: {ex.Message} - DELETE() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }

        /// <summary>
        /// Deleta município por id.
        /// </summary>
        /// <returns>Não há retorno.</returns>
        /// <response code="204">Município deletado com sucesso do banco de dados.</response>
        /// <response code="404">Nenhum dado encontrado no banco de dados.</response>
        /// <response code="400">Erro na requisição.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteById(int id)
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
                        // Busca um município por id no banco de dados
                        var municipioExistente = context.Municipios.Find(id);

                        if (municipioExistente == null)
                        {
                            _logger.LogInformation($"Nenhum município encontrado com o ID: {id}.");
                            return NotFound();
                        }

                        // Se existir o município na base, efetua a deleção
                        context.Municipios.Remove(municipioExistente);
                        context.SaveChanges();

                        _logger.LogInformation($"Sucesso ao deletar o município com o ID: {id}.");
                        return NoContent();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao deletar o município com ID: {id} - Erro: {ex.Message} - DELETEBYID() in MunicipiosController");
                    return BadRequest(ex);
                }
            };
        }
    }
}
