using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;

public class SessionManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _connectionString;

    public SessionManager(IHttpContextAccessor httpContextAccessor, string connectionString)
    {
        _httpContextAccessor = httpContextAccessor;
        _connectionString = connectionString;
    }

    // Método para establecer un valor en la sesión
    public void SetSession(string key, object value)
    {
        _httpContextAccessor.HttpContext.Session.SetString(key, value.ToString());
    }

    // Método para obtener un valor de la sesión y convertirlo al tipo deseado
    public T GetSession<T>(string key)
    {
        var value = _httpContextAccessor.HttpContext.Session.GetString(key);
        if (value != null)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        return default(T);
    }

    // Método para eliminar un valor de la sesión
    public void ClearSession(string key)
    {
        _httpContextAccessor.HttpContext.Session.Remove(key);
    }

    // Método para crear una nueva sesión en la base de datos MySQL
    public void CreateSession(int userId)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            var sessionId = Guid.NewGuid().ToString();
            var expiryDateTime = DateTime.Now.AddHours(1); // Tiempo de expiración de la sesión (ajustar según necesidad)
            
            // Consulta SQL para insertar una nueva sesión en la base de datos
            var sqlCommand = "INSERT INTO sessions (SessionID, UserID, ExpiryDateTime) VALUES (@SessionID, @UserID, @ExpiryDateTime)";
            
            using (var cmd = new MySqlCommand(sqlCommand, connection))
            {
                cmd.Parameters.AddWithValue("@SessionID", sessionId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ExpiryDateTime", expiryDateTime);
                cmd.ExecuteNonQuery(); // Ejecuta la consulta para insertar la sesión
            }

            // Almacena el SessionID en la sesión de ASP.NET para identificar la sesión del usuario
            SetSession("SessionID", sessionId);
        }
    }
}

