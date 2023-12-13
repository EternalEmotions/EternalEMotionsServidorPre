using Microsoft.AspNetCore.Mvc;
using Data;
using Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Stripe.Checkout;
using Stripe;
using Microsoft.EntityFrameworkCore;

namespace Classes;


[ApiController]
 [Route("[Controller]")]
public class ReservasController : ControllerBase
{
    private readonly DataContext _context;

    public ReservasController(DataContext dataContext)
    {
        _context = dataContext;
    }

    // Obtener todas las reservas con información del producto
    [HttpGet]
    public IActionResult GetReservas()
    {
        var reservas = _context.Reservas.Include(r => r.evento).ToList();
        return Ok(reservas);
    }

    // Obtener una reserva por su Id
    [HttpGet("{id}")]
    public IActionResult GetReserva(int id)
    {
        var reserva = _context.Reservas.Include(r => r.evento).FirstOrDefault(r => r.id == id);

        if (reserva == null)
        {
            return NotFound();
        }

        return Ok(reserva);
    }

    // Crear una reserva
 [HttpPost]
public IActionResult CrearReserva([FromBody] Reserva reserva)
{
    // Verificar la disponibilidad y la hora permitida
    if (!EsFechaYHoraDisponible(reserva.eventoId, reserva.fecha, reserva.hora) || !EsHoraPermitida(reserva.hora))
    {
        return BadRequest("La hora no está disponible para este evento en la fecha seleccionada o no es una hora permitida.");
    }

    // Asociar la reserva con el evento utilizando el id proporcionado
    if (reserva.eventoId > 0)
    {
        reserva.evento = _context.Eventos.FirstOrDefault(e => e.id == reserva.eventoId);
    }

    // Guardar la reserva en la base de datos
    _context.Reservas.Add(reserva);
    _context.SaveChanges();

    return Ok(reserva);
}

// Método para verificar si la fecha y hora están disponibles para el evento
private bool EsFechaYHoraDisponible(int eventoId, string fecha, string hora)
{
    // Verificar si ya existe una reserva para la misma fecha y hora en este evento
    return !_context.Reservas.Any(r => r.eventoId == eventoId && r.fecha == fecha && r.hora == hora);
}

// Método para verificar si la hora está en el conjunto permitido
private bool EsHoraPermitida(string hora)
{
    // Lista de horas permitidas
    List<string> horasPermitidas = new List<string> { "10:00", "12:00", "17:00", "19:00" };

    // Verificar si la hora está en la lista de horas permitidas
    return horasPermitidas.Contains(hora);
}
[HttpGet("{eventoId}/{fecha}")]
public IActionResult GetHorasDisponibles(int eventoId, string fecha)
{
    // Obtener todas las reservas para el evento y fecha específicos
    var reservas = _context.Reservas.Where(r => r.eventoId == eventoId && r.fecha == fecha).ToList();

    // Lista de horas permitidas
    List<string> horasPermitidas = new List<string> { "10:00", "12:00", "17:00", "19:00" };

    // Filtrar las horas disponibles
    var horasDisponibles = horasPermitidas.Except(reservas.Select(r => r.hora)).ToList();

    return Ok(horasDisponibles);
}




[HttpDelete("{id}")]
public IActionResult EliminarReserva(int id)
{
    var reserva = _context.Reservas.Find(id);

    if (reserva == null)
    {
        return NotFound();
    }

    _context.Reservas.Remove(reserva);
    _context.SaveChanges();

    return Ok("Reserva eliminada exitosamente");
}

[HttpPut("{id}")]
public IActionResult ActualizarReserva(int id, [FromBody] Reserva reservaActualizada)
{
    var reserva = _context.Reservas.Find(id);

    if (reserva == null)
    {
        return NotFound();
    }

    // Verificar la disponibilidad y la hora permitida para la reserva actualizada
    if (!EsFechaYHoraDisponible(reservaActualizada.eventoId, reservaActualizada.fecha, reservaActualizada.hora) || !EsHoraPermitida(reservaActualizada.hora))
    {
        return BadRequest("La hora no está disponible para este evento en la fecha seleccionada o no es una hora permitida.");
    }

    // Actualizar los campos de la reserva existente con la información proporcionada
    reserva.eventoId = reservaActualizada.eventoId;
    reserva.fecha = reservaActualizada.fecha;
    reserva.hora = reservaActualizada.hora;

    // Guardar los cambios en la base de datos
    _context.SaveChanges();

    return Ok(reserva);
}

}
