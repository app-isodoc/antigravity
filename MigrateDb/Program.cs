using Microsoft.Data.Sqlite;

Console.WriteLine("🔧 Corrigindo usuário Demo e criando Cliente Demo...\n");

var connectionString = "DataSource=Isodoc.Web/isodoc.db";

using var connection = new SqliteConnection(connectionString);
connection.Open();

// 1. Verificar se existe Cliente Demo
var checkClientCmd = connection.CreateCommand();
checkClientCmd.CommandText = "SELECT Id FROM Clients WHERE EmailEmpresa = 'admin@demo.com' LIMIT 1";
var existingClientId = checkClientCmd.ExecuteScalar();

string clientId;

if (existingClientId == null)
{
    clientId = Guid.NewGuid().ToString();
    Console.WriteLine($"Criando novo Cliente Demo com ID: {clientId}");
    
    using var insertClientCmd = connection.CreateCommand();
    insertClientCmd.CommandText = @"
        INSERT INTO Clients (Id, NomeFantasia, RazaoSocial, CNPJ, EmailEmpresa, Telefone, Logradouro, Numero, Bairro, Cidade, Estado, CEP, Status, CreatedAt, UrlPersonalizada, ValorMensal, Plano)
        VALUES (@Id, 'Empresa Demo', 'Empresa Demo LTDA', '00.000.000/0001-00', 'admin@demo.com', '(11) 99999-9999', 'Rua Demo', '123', 'Centro', 'São Paulo', 'SP', '00000-000', 'Ativo', @CreatedAt, 'demo.isodoc.com.br', 0, 'Free')";
    
    insertClientCmd.Parameters.AddWithValue("@Id", clientId);
    insertClientCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
    insertClientCmd.ExecuteNonQuery();
}
else
{
    clientId = existingClientId.ToString();
    Console.WriteLine($"Cliente Demo já existe com ID: {clientId}");
}

// 2. Atualizar usuário admin@demo.com com o ClientId
Console.WriteLine("Atualizando usuário admin@demo.com...");
using var updateUserCmd = connection.CreateCommand();
updateUserCmd.CommandText = "UPDATE AspNetUsers SET ClientId = @ClientId WHERE Email = 'admin@demo.com'";
updateUserCmd.Parameters.AddWithValue("@ClientId", clientId);
int rowsAffected = updateUserCmd.ExecuteNonQuery();

if (rowsAffected > 0)
{
    Console.WriteLine("✅ Usuário admin@demo.com atualizado com sucesso!");
}
else
{
    Console.WriteLine("⚠ Usuário admin@demo.com não encontrado.");
}

Console.WriteLine("\n✅ Correção concluída!");
