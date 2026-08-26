using System.ComponentModel.DataAnnotations;

namespace ProductosApiNet8.DTOs;

// Represento los datos que recibo al crear o actualizar un producto.
public class ProductoRequest
{
    // Exijo un nombre entre 2 y 120 caracteres.
    [Required, StringLength(120, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    // Permito una descripción opcional de hasta 500 caracteres.
    [StringLength(500)]
    public string? Descripcion { get; set; }

    // Exijo un precio positivo dentro del rango permitido.
    [Range(0.01, 99999999.99)]
    public decimal Precio { get; set; }
}