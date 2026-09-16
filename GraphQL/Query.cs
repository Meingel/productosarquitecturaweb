using Microsoft.AspNetCore.Mvc;
using ProductosApiNet8.DTOs;
using ProductosApiNet8.Services;

namespace ProductosApiNet8.GraphQL
{
    /// <summary>
    /// Define el punto de entrada para las operaciones de consulta disponibles mediante GraphQL.
    /// 
    /// En GraphQL, una query se utiliza para realizar operaciones de lectura sobre los datos de la aplicación.
    /// Esta clase reutiliza la capa de servicios existentes para evitar accdeder directamente a la base  de datos desde GraphQL.
    /// </summary>  
    public class Query
    {
        /// <summary>
        /// Obtengo todos los productos registrados en la aplicación.
        /// 
        /// El servicio de productos es inyectado automaticamente por Host Chocolate mediante el contenedor de dependencias de .NET.
        /// </summary>
        /// <param name="productoService">
        /// Servicio que contiene la lógica de negocio para consultar productos en la aplicación.
        /// </param>    
        /// <param name="cancellationToken">
        /// Permite cancelar la operación si la solicitus HTTP es interrumpida.
        /// </param>
        /// <returns>
        /// Lista con todos los productos registrados.
        /// </returns>
        public async Task<IReadOnlyList<ProductoResponse>> GetProductos(
            [Service] IProductoService productoService, CancellationToken cancellationToken)
        {
            // Delego la consulta de productos al servicio de productos.
            // para mantener la separación de responsabilidades de la aplicación
            return await productoService.ObtenerTodosAsync(cancellationToken);
        }

        /// <summary>
        /// Obtengo un producto registrados en la aplicación a partir de su identificador.
        /// 
        /// Este métopdo permite realizar una consulta GraphQL parametrizada, reutilizando la capa de servicios existente de la aplicación.
        /// </summary>
        /// <param name="id">
        /// Identificador único del producto que se desea consultar.
        /// </param>
        /// <param name="productoService">
        /// Servicio que contiene la lógica de negocio para consultar productos en la aplicación.
        /// </param>    
        /// <param name="cancellationToken">
        /// Permite cancelar la operación si la solicitus HTTP es interrumpida.
        /// </param>
        /// <returns>
        /// El producto encontrado o un error NOT_FOUND si no existe el identificador.
        /// </returns>
        public async Task<ProductoResponse?> GetProductoPorId(
            int id,
            [Service] IProductoService productoService, CancellationToken cancellationToken)
        {
            // Delego la consulta de productos al servicio de productos.
            // para mantener la separación de responsabilidades de la aplicación
            return await productoService.ObtenerPorIdAsync(id, cancellationToken)
                ?? throw new GraphQLException(ErrorBuilder.New().SetMessage("El producto no existe.").SetCode("NOT_FOUND").Build());
        }

    }
}
