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

 [Route("[Controller]")]
[ApiController]
public class EventosController : ControllerBase
{
    private readonly DataContext _context;

    public EventosController(DataContext dataContext)
    {
        _context = dataContext;
    }

    // GET: api/eventos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Evento>>> GetEventos()
    {
        return await _context.Eventos.ToListAsync();
    }

    // GET: api/eventos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Evento>> GetEvento(int id)
    {
        var evento = await _context.Eventos.FindAsync(id);

        if (evento == null)
        {
            return NotFound();
        }

        return evento;
    }

    // POST: api/eventos
    [HttpPost]
    public async Task<ActionResult<Evento>> PostEvento(Evento evento)
    {
        _context.Eventos.Add(evento);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetEvento", new { id = evento.id }, evento);
    }

    // PUT: api/eventos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEvento(int id, Evento evento)
    {
        if (id != evento.id)
        {
            return BadRequest();
        }

        _context.Entry(evento).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EventoExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/eventos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvento(int id)
    {
        var evento = await _context.Eventos.FindAsync(id);
        if (evento == null)
        {
            return NotFound();
        }

        _context.Eventos.Remove(evento);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EventoExists(int id)
    {
        return _context.Eventos.Any(e => e.id == id);
    }
}
