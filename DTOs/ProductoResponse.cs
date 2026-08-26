namespace ProductosApiNet8.DTOs;

// Devuelvo únicamente los datos públicos del producto.
public record ProductoResponse(int Id, string Nombre, string? Descripcion, decimal Precio);