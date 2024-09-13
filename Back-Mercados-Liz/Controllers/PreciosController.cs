using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

namespace MyApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreciosController : ControllerBase
    {
        private readonly string _connectionString;

        public PreciosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/precios
        [HttpGet]
        public async Task<IActionResult> GetPrecios([FromQuery] string? codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                return BadRequest(new ErrorResponse { Message = "Debe proporcionar el parámetro 'codigo'." });
            }

            var precios = new List<PrecioDto>();
            var ofertas = new List<OfertaDto>();

            // Definir las consultas SQL
            string queryByCodigo = @"
            SELECT 
                CB.Codigo, 
                CB.Cuenta, 
                ART.Descripcion1, 
                ListaPreciosDUnidad.Unidad, 
                ListaPreciosDUnidad.Precio,
                ArtUnidad.Factor 
            FROM CB 
                INNER JOIN ART ON CB.Cuenta = ART.Articulo 
                INNER JOIN ListaPreciosDUnidad ON CB.Cuenta = ListaPreciosDUnidad.Articulo 
                INNER JOIN ArtUnidad ON CB.Cuenta = ArtUnidad.Articulo 
            WHERE 
                ListaPreciosDUnidad.Lista = '(PRECIO 3)' 
                AND CB.Unidad = ListaPreciosDUnidad.UNIDAD 
                AND CB.Unidad = ArtUnidad.Unidad 
                AND CB.Codigo = @Codigo 
            ORDER BY ArtUnidad.Factor ASC;
        ";

            string queryByArticulo = @"
            SELECT 
                CB.Codigo, 
                CB.Cuenta, 
                ART.Descripcion1, 
                ListaPreciosDUnidad.Unidad, 
                ListaPreciosDUnidad.Precio,
                ArtUnidad.Factor 
            FROM CB 
                INNER JOIN ART ON CB.Cuenta = ART.Articulo 
                INNER JOIN ListaPreciosDUnidad ON CB.Cuenta = ListaPreciosDUnidad.Articulo 
                INNER JOIN ArtUnidad ON CB.Cuenta = ArtUnidad.Articulo 
            WHERE 
                ListaPreciosDUnidad.Lista = '(PRECIO 3)' 
                AND CB.Unidad = ListaPreciosDUnidad.UNIDAD 
                AND CB.Unidad = ArtUnidad.Unidad 
                AND ART.Articulo = (SELECT Articulo FROM ART WHERE Articulo = @Codigo)
            ORDER BY ArtUnidad.Factor ASC;
        ";

            string query_oferta = @"
            SELECT 
                OfertaD.Articulo,
                OfertaD.Precio,
                Oferta.FechaD,
                Oferta.FechaA
            FROM 
                OfertaD 
                INNER JOIN Oferta ON OfertaD.ID = Oferta.ID
            WHERE
                OfertaD.Articulo = @Codigo
                AND
                Oferta.FechaD < GETDATE() 
                AND
                Oferta.FechaA > GETDATE();
        ";

            try
            {
                await using var connection = OpenConnection();

                // Primero buscar con la consulta principal usando el 'codigo'
                await using var command = new SqlCommand(queryByCodigo, connection);
                command.Parameters.AddWithValue("@Codigo", codigo);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    precios.Add(MapToPrecioDto(reader));
                }
                reader.Close();

                // Si no se encuentra ningún resultado, buscar con la consulta alternativa
                if (!precios.Any())
                {
                    await using var commandAlt = new SqlCommand(queryByArticulo, connection);
                    commandAlt.Parameters.AddWithValue("@Codigo", codigo);

                    await using var readerAlt = await commandAlt.ExecuteReaderAsync();
                    while (await readerAlt.ReadAsync())
                    {
                        precios.Add(MapToPrecioDto(readerAlt));
                    }
                }

                // Buscar ofertas
                await using var commandOferta = new SqlCommand(query_oferta, connection);
                commandOferta.Parameters.AddWithValue("@Codigo", codigo);

                await using var readerOferta = await commandOferta.ExecuteReaderAsync();
                while (await readerOferta.ReadAsync())
                {
                    ofertas.Add(MapToOfertaDto(readerOferta)); // Utiliza el método MapToOfertaDto correcto
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

            // Devolver los resultados encontrados
            return Ok(new { Precios = precios, Ofertas = ofertas });
        }



        // POST: api/precios
        [HttpPost]
        public async Task<IActionResult> CreatePrecio([FromBody] PrecioDto precioDto)
        {
            if (precioDto == null)
            {
                return BadRequest(new ErrorResponse { Message = "PrecioDto es nulo." });
            }

            string query = @"
                INSERT INTO 
                    ListaPreciosDUnidad 
                    (Codigo, Cuenta, Descripcion1, Unidad, Precio, Factor) 
                VALUES 
                    (@Codigo, @Cuenta, @Descripcion1, @Unidad, @Precio, @Factor);";

            try
            {
                await using var connection = OpenConnection();
                await using var command = new SqlCommand(query, connection);
                AddParameters(command, precioDto);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return CreatedAtAction(nameof(GetPrecios), new { codigo = precioDto.Codigo }, precioDto);
                }
                else
                {
                    return StatusCode(500, new ErrorResponse { Message = "No se pudo insertar el registro." });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // PUT: api/precios/{codigo}
        [HttpPut("{codigo}")]
        public async Task<IActionResult> UpdatePrecio(string codigo, [FromBody] PrecioDto precioDto)
        {
            if (precioDto == null || codigo != precioDto.Codigo)
            {
                return BadRequest(new ErrorResponse { Message = "Datos inválidos." });
            }

            string query = @"
                UPDATE 
                    ListaPreciosDUnidad 
                SET 
                    Cuenta = @Cuenta, 
                    Descripcion1 = @Descripcion1, 
                    Unidad = @Unidad, 
                    Precio = @Precio, 
                    Factor = @Factor 
                WHERE 
                    Codigo = @Codigo;";

            try
            {
                await using var connection = OpenConnection();
                await using var command = new SqlCommand(query, connection);
                AddParameters(command, precioDto);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return NoContent(); // 204 No Content
                }
                else
                {
                    return NotFound(new ErrorResponse { Message = "Precio no encontrado." });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // DELETE: api/precios/{codigo}
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> DeletePrecio(string codigo)
        {
            string query = "DELETE FROM ListaPreciosDUnidad WHERE Codigo = @Codigo;";

            try
            {
                await using var connection = OpenConnection();
                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Codigo", codigo);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return NoContent(); // 204 No Content
                }
                else
                {
                    return NotFound(new ErrorResponse { Message = "Precio no encontrado." });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.OpenAsync().Wait();
            return connection;
        }

        private void AddParameters(SqlCommand command, PrecioDto precioDto)
        {
            command.Parameters.AddWithValue("@Codigo", precioDto.Codigo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Cuenta", precioDto.Cuenta ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Descripcion1", precioDto.Descripcion1 ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Unidad", precioDto.Unidad ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Precio", precioDto.Precio);
            command.Parameters.AddWithValue("@Factor", precioDto.Factor);
        }

        private PrecioDto MapToPrecioDto(SqlDataReader reader)
        {
            return new PrecioDto
            {
                Codigo = reader["Codigo"] != DBNull.Value ? reader["Codigo"].ToString() : null,
                Cuenta = reader["Cuenta"] != DBNull.Value ? reader["Cuenta"].ToString() : null,
                Descripcion1 = reader["Descripcion1"] != DBNull.Value ? reader["Descripcion1"].ToString() : null,
                Unidad = reader["Unidad"] != DBNull.Value ? reader["Unidad"].ToString() : null,
                Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                Factor = reader["Factor"] != DBNull.Value ? Convert.ToDecimal(reader["Factor"]) : 0,
            };
        }

        private OfertaDto MapToOfertaDto(SqlDataReader reader)
        {
            return new OfertaDto
            {
                Articulo = reader["Articulo"].ToString(),
                Precio = Convert.ToDecimal(reader["Precio"]),
                FechaDesde = Convert.ToDateTime(reader["FechaD"]),
                FechaHasta = Convert.ToDateTime(reader["FechaA"])
            };
        }
        private IActionResult HandleException(Exception ex)
        {
            return StatusCode(500, new ErrorResponse { Message = "Error: " + ex.Message });
        }
    }
}
