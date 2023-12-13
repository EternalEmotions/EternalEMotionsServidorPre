//Clase
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Classes;

public class Reserva{
    public int id {get;set;}
    public int eventoId{get;set;}
     [ForeignKey("eventoId")]
        public Evento? evento { get; set; }
    public string fecha{get;set;}
    public string hora{get;set;}


}