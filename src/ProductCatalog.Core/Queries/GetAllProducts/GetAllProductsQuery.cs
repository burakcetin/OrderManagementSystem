using MediatR;
using ProductCatalog.Core.Models;

namespace ProductCatalog.Core.Queries.GetAllProducts
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
        public string? Category { get; set; }
    }
}
