using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Productos.Grpc.Services;

// Adapto los errores de negocio y persistencia a los códigos propios del protocolo gRPC.
public sealed class ErroresInterceptor(ILogger<ErroresInterceptor> logger) : Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        => Ejecutar(() => continuation(request, context), context);

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request, IServerStreamWriter<TResponse> stream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
        => await Ejecutar(async () => { await continuation(request, stream, context); return true; }, context);

    private async Task<T> Ejecutar<T>(Func<Task<T>> accion, ServerCallContext context)
    {
        try { return await accion(); }
        catch (RpcException) { throw; }
        catch (ValidationException ex) { throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message)); }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        { throw new RpcException(new Status(StatusCode.Cancelled, "Solicitud cancelada.")); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al procesar una operación gRPC.");
            throw new RpcException(new Status(StatusCode.Internal, "No fue posible completar la operación."));
        }
    }
}
