namespace ApiTarefas.Models;

using System.Security.Cryptography;
using System.IO;

public class Gerador
{
    public static void GerarChave()
    {
        if (File.Exists(".env"))
        {
            foreach (string line in File.ReadLines(".env"))
            {
                if (line.StartsWith("ChaveSecreta=")) return;
            }
        }
        using var writer = File.AppendText(".env");
        writer.WriteLine($"ChaveSecreta={Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))}");
    }
}