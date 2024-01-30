using Newtonsoft.Json;


public class Registrar{

    public static List<General> ListaIntercambio = new List<General>();

    public static void Aplicar(ref WebApplication app){

        var TagNamee = "Registro de intercambios de disparos";

        //agregar intercambios
        app.MapPost("/crud/agregar", Agregar)
        .Produces<ServerResult>().WithTags(TagNamee)
        .WithDescription("Agregar un intercambios de disparo a la lista");

        //listar intercambios
        app.MapGet("/crud/listar", Listar)
        .Produces<ServerResult>().WithTags(TagNamee)
        .WithDescription("Listar todos los intercambios registrados");

        //buscar intercambios
        app.MapPut("/crud/Consulta", ConsultarPersona)
        .Produces<ServerResult>().WithTags(TagNamee)
        .WithDescription("Buscar el intercambio en que esta involucrado esa persona");
        
    }

    public static ServerResult Agregar(General general)
    {
        var resultado = new ServerResult();

        try
        {
            if (!Directory.Exists("datax"))
            {
                Directory.CreateDirectory("datax");
            }

            if (general.participantes.Any(p => string.IsNullOrWhiteSpace(p.cedula) || p.cedula.Length != 11))
            {
                resultado.Exito = false;
                resultado.Mensaje = "Una o más cédulas en los participantes no son válidas.";
                return resultado;
            }

            if (ListaIntercambio.Contains(general))
            {
                resultado.Exito = false;
                resultado.Mensaje = "Intercambio ya existe";
                return resultado;
            }

            string json = JsonConvert.SerializeObject(general);

            string filePath = Path.Combine("datax", $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json");
            File.WriteAllText(filePath, json);

            ListaIntercambio.Add(general);

            resultado.Exito = true;
            resultado.Resultado = general;
            resultado.Mensaje = "Intercambio agregado con éxito";
        }
        catch (Exception ex)
        {
            resultado.Exito = false;
            resultado.Mensaje = $"Error al agregar el intercambio: {ex.Message}";
        }

        return resultado;
    }

    public static ServerResult Listar(DateTime fechaInicio, DateTime fechaFin)
    {
        var resultado = new ServerResult();

        try
        {
            string[] archivos = Directory.GetFiles("datax");

            List<General> intercambiosEnRango = new List<General>();

            foreach (var archivo in archivos)
            {
                string jsonData = File.ReadAllText(archivo);

                General intercambio = JsonConvert.DeserializeObject<General>(jsonData)!;

                if (intercambio.fecha >= fechaInicio && intercambio.fecha <= fechaFin)
                {
                    intercambiosEnRango.Add(intercambio);
                }
            }

            if (intercambiosEnRango.Count > 0)
            {
                resultado.Resultado = intercambiosEnRango;
                resultado.Mensaje = "Intercambios listados con éxito";
            }
            else
            {
                resultado.Mensaje = "No hay intercambios dentro del rango de fechas";
            }
        }
        catch (Exception ex)
        {
            resultado.Exito = false;
            resultado.Mensaje = $"Error al listar los intercambios: {ex.Message}";
        }

        return resultado;
    }


    public static ServerResult ConsultarPersona(string cedula)
    {
        var resultado = new ServerResult();

        try
        {
           var intercambiosInvolucrados = ListaIntercambio
                .Where(e => e.participantes != null && e.participantes.Any(p => p.cedula.Equals(cedula, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (intercambiosInvolucrados.Count > 0)
            {
                var rolesPorIntercambio = intercambiosInvolucrados
                    .Select(e => new
                    {
                        Intercambio = e,
                        Rol = e.participantes.First(p => p.cedula.Equals(cedula, StringComparison.OrdinalIgnoreCase))?.rol
                    })
                    .ToList();

                resultado.Resultado = rolesPorIntercambio;
                resultado.Mensaje = $"La persona con cédula {cedula} está involucrada en {rolesPorIntercambio.Count} intercambios.";
            }
            else
            {
                resultado.Exito = false;
                resultado.Mensaje = $"La persona con cédula {cedula} no está involucrada en ningún intercambio.";
            }
        }
        catch (Exception ex)
        {
            resultado.Exito = false;
            resultado.Mensaje = $"Error al consultar la persona: {ex.Message}";
            Console.WriteLine(ex.StackTrace); 
        }

        return resultado;
    }


}


public class Coordenadas
{
    public double latitud {get; set;}
    public double longitud {get; set;}
}

public class Herido
{
    public string cedula {get; set;} = string.Empty;
    public string nombre {get; set;} = string.Empty;
}

public class Muerto
{
    public string cedula {get; set;} = string.Empty;
    public string nombre {get; set;} = string.Empty;
}

public class Participante
{
    public string cedula {get; set;} = string.Empty;
    public string nombre {get; set;} = string.Empty;
    public string rol {get; set;} = string.Empty;
}

public class General
{
    public DateTime fecha {get; set;}
    public string lugar {get; set;} = string.Empty;
    public Coordenadas coordenadas {get; set;} = null!;
    public List<Participante> participantes {get; set;} = null!; 
    public List<Muerto> muertos {get; set;} = null!;
    public List<Herido> heridos {get; set;} = null!;
}