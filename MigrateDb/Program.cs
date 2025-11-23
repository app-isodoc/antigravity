using Microsoft.Data.Sqlite;

Console.WriteLine("🔧 Atualizando esquema do banco de dados para ISO 9001 Workflow...\n");

var connectionString = "DataSource=Isodoc.Web/isodoc.db";

using var connection = new SqliteConnection(connectionString);
connection.Open();

var commands = new[]
{
    "ALTER TABLE NonConformities ADD COLUMN ResponsavelContencaoId TEXT;",
    "ALTER TABLE NonConformities ADD COLUMN ResponsavelAnaliseCausaId TEXT;",
    "ALTER TABLE NonConformities ADD COLUMN ResponsavelAcaoCorretivaId TEXT;",
    "ALTER TABLE NonConformities ADD COLUMN ResponsavelVerificacaoId TEXT;",
    "ALTER TABLE NonConformities ADD COLUMN ContencaoConcluida INTEGER NOT NULL DEFAULT 0;",
    "ALTER TABLE NonConformities ADD COLUMN AnaliseCausaConcluida INTEGER NOT NULL DEFAULT 0;",
    "ALTER TABLE NonConformities ADD COLUMN AcaoCorretivaConcluida INTEGER NOT NULL DEFAULT 0;",
    "ALTER TABLE NonConformities ADD COLUMN VerificacaoConcluida INTEGER NOT NULL DEFAULT 0;",
    
    // Recriar Tabela de Notificações para garantir estrutura correta
    "DROP TABLE IF EXISTS Notifications;",
    @"CREATE TABLE Notifications (
        Id TEXT PRIMARY KEY,
        UserId TEXT NOT NULL,
        Title TEXT NOT NULL,
        Message TEXT NOT NULL,
        Link TEXT,
        IsRead INTEGER NOT NULL DEFAULT 0,
        CreatedAt TEXT NOT NULL,
        FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
    );",

    // Novas colunas para Providências em Ações
    "ALTER TABLE NonConformityActions ADD COLUMN Providencias TEXT;",
    "ALTER TABLE NonConformityActions ADD COLUMN DataProvidencias TEXT;",
    "ALTER TABLE NonConformityActions ADD COLUMN UsuarioProvidenciasId TEXT;"
};

foreach (var cmdText in commands)
{
    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = cmdText;
        command.ExecuteNonQuery();
        Console.WriteLine($"✅ Sucesso: {cmdText}");
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 1) // Error 1: duplicate column name
    {
        Console.WriteLine($"ℹ️ Coluna já existe (ignorado): {cmdText}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao executar '{cmdText}': {ex.Message}");
    }
}

Console.WriteLine("\n✅ Migração concluída!");
