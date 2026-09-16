using System.ComponentModel.DataAnnotations;

namespace ProductosApiNet8.GraphQL;

// Traduzco las excepciones a errores GraphQL con códigos que el cliente puede interpretar.
public sealed class ProductoErrorFilter(ILogger<ProductoErrorFilter> logger) : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ValidationException validation)
            return error.WithMessage(validation.Message).WithCode("BAD_USER_INPUT");
        if (error.Exception is not null)
        {
            logger.LogError(error.Exception, "Error al procesar una operación GraphQL.");
            return error.WithMessage("No fue posible completar la operación.").WithCode("INTERNAL_SERVER_ERROR");
        }
        return error;
    }
}
