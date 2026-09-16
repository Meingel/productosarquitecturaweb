using System.Collections.Concurrent;
using System.Threading.Channels;
using Productos.Grpc.Contratos;

namespace Productos.Grpc.Services;

// Cada cliente tiene su propio canal; el evento se distribuye a todos los observadores conectados.
public sealed class ProductoEventos
{
    private readonly ConcurrentDictionary<Guid, Channel<ProductoEvento>> clientes = new();

    public (Guid Id, ChannelReader<ProductoEvento> Reader) Suscribir()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateBounded<ProductoEvento>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });
        clientes[id] = channel;
        return (id, channel.Reader);
    }

    public void Retirar(Guid id)
    {
        if (clientes.TryRemove(id, out var channel)) channel.Writer.TryComplete();
    }

    public void Publicar(string operacion, int id)
    {
        foreach (var channel in clientes.Values)
            channel.Writer.TryWrite(new ProductoEvento { Operacion = operacion, Id = id });
    }
}
