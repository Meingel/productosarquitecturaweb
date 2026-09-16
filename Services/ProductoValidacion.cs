using System.ComponentModel.DataAnnotations;
using ProductosApiNet8.DTOs;

namespace ProductosApiNet8.Services;

// Centralizo las reglas para que REST, GraphQL y gRPC validen exactamente los mismos datos.
public static class ProductoValidacion
{
    public static void Validar(ProductoRequest request)
    {
        request.Nombre = request.Nombre?.Trim() ?? string.Empty;
        request.Descripcion = request.Descripcion?.Trim();
        var resultados = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), resultados, true))
            throw new ValidationException(string.Join(" ", resultados.Select(r => r.ErrorMessage)));
        if (decimal.Round(request.Precio, 2) != request.Precio)
            throw new ValidationException("El precio debe tener como máximo dos decimales.");
    }

    public static void ValidarId(int id)
    {
        if (id <= 0) throw new ValidationException("El identificador debe ser mayor que cero.");
    }
}
