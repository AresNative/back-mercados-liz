public class PrecioDto
{
    public string Codigo { get; set; }
    public string Cuenta { get; set; }
    public string Descripcion1 { get; set; }
    public string Unidad { get; set; }

    // Cambiado de decimal a string para almacenar el precio formateado en MXN
    public string PrecioMXN { get; set; }
    public decimal Precio { get; set; }

    public decimal Factor { get; set; }

    // Nuevas propiedades para las fechas de oferta
    public string FechaD { get; set; }  // Fecha de inicio de la oferta
    public string FechaA { get; set; }  // Fecha de fin de la oferta
}
public class PrecioPostDto
{
    public string Articulo { get; set; }
    public string Lista { get; set; }
}
public class OfertaDto
{
    public string Articulo { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
}
public class GetPreciosRequest
{
    public string? Articulo { get; set; }
    public string? Codigo { get; set; }
}
