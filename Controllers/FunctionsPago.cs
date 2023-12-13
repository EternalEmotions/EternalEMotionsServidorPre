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

namespace Classes
{
    [ApiController]
    [Route("[Controller]")]
    public class PagosController : ControllerBase
    {
        private readonly DataContext _context;

        public PagosController(DataContext dataContext)
        {
            _context = dataContext;

            // Configura tu clave de API de Stripe
            StripeConfiguration.ApiKey = "sk_test_51OKO1bLkVoGrpMmaMHXpcOmlq3e9mG4H8sMpUIHQGLcNYISgq9EZohU3VkvspeGmyDDJpEI85QJBYAQ7e05sBoz2006yRX5cR8";
        }

        [HttpGet]
        public ActionResult<List<Pago>> Get()
        {
            List<Pago> pagos = _context.Pagoss.OrderByDescending(x => x.id).ToList();
            return Ok(pagos);
        }

        [HttpPost]
        public ActionResult Post([FromBody] Pago pago)
        {
            try
            {
                _context.Pagoss.Add(pago);
                _context.SaveChanges();

                EnviarCorreoAdministrador(pago);
                EnviarCorreoUsuario(pago);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private void EnviarCorreoAdministrador(Pago pago)
        {
            string adminEmail = "eternalemotions@hotmail.com";
            string adminSubject = "Nuevo usuario registrado (Administrador)";
            string adminBody = $"Se ha registrado un nuevo usuario con los siguientes datos:\n\n" +
                               $"ID: {pago.id}\n" +
                               $"Nombre: {pago.name}\n" +
                               $"Correo Electrónico: {pago.email}\n" +
                               $"Total: {pago.total}\n"+
                               $"Id Sesion: {pago.StripeSessionId}\n";
                             

            EnviarCorreo(adminEmail, adminSubject, adminBody);
        }

        private void EnviarCorreoUsuario(Pago pago)
        {
            string userEmail = pago.email;
            string userSubject = "Registro exitoso";
            string userBody = $"¡Gracias por registrarte, {pago.name}!\n\n" +
                              $"Tu registro ha sido exitoso. Agradecemos tu confianza.\n\n" +
                              $"Detalles de tu registro:\n\n" +
                              $"ID: {pago.id}\n" +
                              $"Nombre: {pago.name}\n" +
                              $"Correo Electrónico: {pago.email}\n" +
                              $"Total: {pago.total}\n"+
                              $"Id Sesion: {pago.StripeSessionId}\n";

            EnviarCorreo(userEmail, userSubject, userBody);
        }

        private void EnviarCorreo(string recipient, string subject, string body)
        {
            using (SmtpClient smtpClient = new SmtpClient("smtp.office365.com"))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("eternalemotions@hotmail.com", "eternal_Emotions23");
                smtpClient.EnableSsl = true;
                smtpClient.Port = 587;

                using (MailMessage message = new MailMessage("eternalemotions@hotmail.com", recipient, subject, body))
                {
                    smtpClient.Send(message);
                }
            }
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            else
            {
                Pago pagoToDelete = _context.Pagoss.Find(id);
                if (pagoToDelete == null)
                {
                    return NotFound("Pago no encontrado");
                }

                _context.Pagoss.Remove(pagoToDelete);
                _context.SaveChanges();

                var orders = _context.OrderPro.ToList();
                orders.ForEach(o =>
                {
                    if (o.pagoId == id)
                    {
                        _context.OrderPro.Remove(o);
                    }
                });

                _context.SaveChanges();
                
                return Ok();
            }
        }
/*
        [HttpPost("CalculateTotal")]
        public ActionResult<decimal> CalculateTotal([FromBody] List<int> presetIds)
        {
            try
            {
                if (presetIds == null || presetIds.Count == 0)
                {
                    return BadRequest("La lista de IDs de preset no puede estar vacía.");
                }

                var presets = _context.Presets.Where(p => presetIds.Contains(p.id)).ToList();

                if (presets.Count != presetIds.Count)
                {
                    return NotFound("No se encontraron todos los presets proporcionados.");
                }

                decimal total = presets.Sum(p => p.precio);

                return Ok(total);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al procesar la solicitud: " + ex.Message);
            }
        }*/

[HttpPost("CreateStripeSession")]
public async Task<ActionResult<string>> CreateStripeSession([FromBody] Pago pago)
{
    try
    {
        
        // Guarda el pago en la base de datos para obtener el ID del pago
        _context.Pagoss.Add(pago);
        _context.SaveChanges();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Producto",
                        },
                        UnitAmount = (long)(pago.total * 100),
                    },
                    Quantity = 1,
                },
            },
            Mode = "payment",
            SuccessUrl = "http://localhost:8080/#/",
            CancelUrl = "https://tu-sitio.com/cancel",
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // Asigna el ID de la sesión de Stripe al pago
        pago.StripeSessionId = session.Id;

        // Actualiza la entidad Pago en la base de datos con el StripeSessionId
        _context.Pagoss.Update(pago);
        _context.SaveChanges();

        EnviarCorreoUsuario(pago);
        return Ok(new { sessionId = session.Id });
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Error al crear la sesión de Stripe: {ex.Message}");
    }
}

}
}