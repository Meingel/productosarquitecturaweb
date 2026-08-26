namespace ProductosApiNet8.Models;

// Represento el producto que persisto en la base de datos.
public class Producto
{
    // Identifico cada producto con una clave primaria autoincremental.
    public int Id { get; set; }

    // Almaceno el nombre obligatorio del producto.
    public required string Nombre { get; set; }

    // Almaceno una descripción opcional del producto.
    public string? Descripcion { get; set; }

    // Almaceno el precio con precisión monetaria.
    public decimal Precio { get; set; }
}