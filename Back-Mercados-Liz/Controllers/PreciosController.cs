using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreciosController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=Caja06;User Id=sis;Password=t;TrustServerCertificate=True;Encrypt=False;";

        // GET: api/precios
        [HttpGet]
        public async Task<IActionResult> GetPrecios()
        {
            var precios = new List<PrecioDto>();

            string query = "SELECT CB.Codigo, CB.Cuenta, ART.Descripcion1, ListaPreciosDUnidad.Unidad, ListaPreciosDUnidad.Precio, ArtUnidad.Factor FROM CB INNER JOIN ART ON CB.Cuenta = ART.Articulo INNER JOIN ListaPreciosDUnidad ON CB.Cuenta = ListaPreciosDUnidad.Articulo INNER JOIN ArtUnidad ON CB.Cuenta = ArtUnidad.Articulo WHERE Lista = '(PRECIO Lista)' AND ArtUnidad.Articulo = '2001' AND CB.Unidad = ListaPreciosDUnidad.UNIDAD AND CB.Unidad = ArtUnidad.Unidad ORDER BY ArtUnidad.Factor Asc;";

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var precio = new PrecioDto
                                {
                                    Codigo = reader["Codigo"] != DBNull.Value ? reader["Codigo"].ToString() : null,
                                    Cuenta = reader["Cuenta"] != DBNull.Value ? reader["Cuenta"].ToString() : null,
                                    Descripcion1 = reader["Descripcion1"] != DBNull.Value ? reader["Descripcion1"].ToString() : null,
                                    Unidad = reader["Unidad"] != DBNull.Value ? reader["Unidad"].ToString() : null,
                                    Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                                    Factor = reader["Factor"] != DBNull.Value ? Convert.ToDecimal(reader["Factor"]) : 0,
                                };
                                precios.Add(precio);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Error: " + ex.Message });
                }
            }

            return Ok(precios);
        }

        // POST: api/precios
        [HttpPost]
        public async Task<IActionResult> CreatePrecio([FromBody] PrecioDto precioDto)
        {
            if (precioDto == null)
            {
                return BadRequest(new ErrorResponse { Message = "PrecioDto es nulo." });
            }

            string query = "INSERT INTO ListaPreciosDUnidad (Codigo, Cuenta, Descripcion1, Unidad, Precio, Factor) VALUES (@Codigo, @Cuenta, @Descripcion1, @Unidad, @Precio, @Factor);";

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Codigo", precioDto.Codigo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Cuenta", precioDto.Cuenta ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Descripcion1", precioDto.Descripcion1 ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Unidad", precioDto.Unidad ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Precio", precioDto.Precio);
                        command.Parameters.AddWithValue("@Factor", precioDto.Factor);

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
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Error: " + ex.Message });
                }
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

            string query = "UPDATE ListaPreciosDUnidad SET Cuenta = @Cuenta, Descripcion1 = @Descripcion1, Unidad = @Unidad, Precio = @Precio, Factor = @Factor WHERE Codigo = @Codigo;";

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Codigo", precioDto.Codigo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Cuenta", precioDto.Cuenta ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Descripcion1", precioDto.Descripcion1 ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Unidad", precioDto.Unidad ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Precio", precioDto.Precio);
                        command.Parameters.AddWithValue("@Factor", precioDto.Factor);

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
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Error: " + ex.Message });
                }
            }
        }

        // DELETE: api/precios/{codigo}
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> DeletePrecio(string codigo)
        {
            string query = "DELETE FROM ListaPreciosDUnidad WHERE Codigo = @Codigo;";

            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    await using (SqlCommand command = new SqlCommand(query, connection))
                    {
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
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ErrorResponse { Message = "Error: " + ex.Message });
                }
            }
        }
    }
}
