using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TiendaServicios.Api.CarritoCompra.Persistencia;
using TiendaServicios.Api.CarritoCompra.RemoteInterface;

namespace TiendaServicios.Api.CarritoCompra.Aplicacion
{
    public class Consulta
    {
        public class Ejecuta : IRequest<CarritoDTO>
        {
            public int CarritoSesionId { get; set; }
        }

        public class Manejador : IRequestHandler<Ejecuta, CarritoDTO>
        {
            private readonly CarritoContexto _carritoContexto;
            private readonly ILibrosServices _libroServices;

            public Manejador(CarritoContexto carritoContexto, ILibrosServices librosServices)
            {
                _carritoContexto = carritoContexto;
                _libroServices = librosServices;
            }

            public async Task<CarritoDTO> Handle(Ejecuta request, CancellationToken cancellationToken)
            {
                var carritoSesion = await _carritoContexto.CarritoSesion.FirstOrDefaultAsync(x => x.CarritoSesionId == request.CarritoSesionId);
                var carritoSesionDetalle = await _carritoContexto.CarritoSesionDetalle.Where(x => x.CarritoSesionId == request.CarritoSesionId).ToListAsync();

                var listaCarritoDto = new List<CarritoDetalleDTO>();

                foreach(var libro in carritoSesionDetalle)
                {
                   var response = await _libroServices.GetLibro(new Guid(libro.ProductoSeleccionado));
                    if (response.resultado)
                    {
                        var objetoLibro = response.Libro;
                        var carritoDetalle = new CarritoDetalleDTO
                        {
                            TituloLibro = objetoLibro.Titulo,
                            FechaPublicacion = objetoLibro.FechaPublicacion,
                            LibroId = objetoLibro.LibreriaMaterialId
                        };
                        listaCarritoDto.Add(carritoDetalle);
                    }
                }

                var carritoSesionDto = new CarritoDTO
                {
                    CarritoId = carritoSesion.CarritoSesionId,
                    FechaCreacionSesion = carritoSesion.FechaCreacion,
                    ListaProductos = listaCarritoDto
                };

                return carritoSesionDto;

            }
        }
    }
}
