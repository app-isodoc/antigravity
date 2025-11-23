using Microsoft.Data.Sqlite;

var connectionString = "DataSource=isodoc.db";

using var connection = new SqliteConnection(connectionString);
connection.Open();

var sql = @"
-- Adicionar novas colunas à tabela NonConformities
ALTER TABLE NonConformities ADD COLUMN Titulo TEXT;
ALTER TABLE NonConformities ADD COLUMN Numero TEXT NOT NULL DEFAULT '';
ALTER TABLE NonConformities ADD COLUMN Severidade TEXT DEFAULT 'Média';
ALTER TABLE NonConformities ADD COLUMN Departamento TEXT;
ALTER TABLE NonConformities ADD COLUMN ClienteEnvolvidoId TEXT;
ALTER TABLE NonConformities ADD COLUMN TipoIncidente TEXT;
ALTER TABLE NonConformities ADD COLUMN NormaIsoId TEXT;
ALTER TABLE NonConformities ADD COLUMN PrazoConclusao TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelContencaoId TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelAnaliseId TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelCorretivasId TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelVerificacaoId TEXT;
ALTER TABLE NonConformities ADD COLUMN AcoesContencao TEXT;
ALTER TABLE NonConformities ADD COLUMN DataContencao TEXT;
ALTER TABLE NonConformities ADD COLUMN AnaliseCausa TEXT;
ALTER TABLE NonConformities ADD COLUMN CausaRaiz TEXT;
ALTER TABLE NonConformities ADD COLUMN DataAnalise TEXT;
ALTER TABLE NonConformities ADD COLUMN AcoesCorretivas TEXT;
ALTER TABLE NonConformities ADD COLUMN DataCorretivas TEXT;
ALTER TABLE NonConformities ADD COLUMN ResultadoVerificacao TEXT;
ALTER TABLE NonConformities ADD COLUMN DataVerificacao TEXT;
ALTER TABLE NonConformities ADD COLUMN EficazVerificacao INTEGER;
ALTER TABLE NonConformities ADD COLUMN Progresso INTEGER DEFAULT 0;
";

var commands = sql.Split(';', StringSplitOptions.RemoveEmptyEntries);

foreach (var cmd in commands)
{
    if (!string.IsNullOrWhiteSpace(cmd))
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = cmd.Trim();
            command.ExecuteNonQuery();
            Console.WriteLine($"✓ Executado: {cmd.Trim().Substring(0, Math.Min(50, cmd.Trim().Length))}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Erro (pode ser ignorado se coluna já existe): {ex.Message}");
        }
    }
}

// Criar novas tabelas
var createTables = @"
CREATE TABLE IF NOT EXISTS NonConformityEvidences (
    Id TEXT PRIMARY KEY,
    NonConformityId TEXT NOT NULL,
    Descricao TEXT NOT NULL,
    ArquivoUrl TEXT,
    ArquivoNome TEXT,
    DataUpload TEXT NOT NULL,
    UploadedById TEXT,
    FOREIGN KEY (NonConformityId) REFERENCES NonConformities(Id) ON DELETE CASCADE,
    FOREIGN KEY (UploadedById) REFERENCES AspNetUsers(Id)
);

CREATE TABLE IF NOT EXISTS NonConformityActions (
    Id TEXT PRIMARY KEY,
    NonConformityId TEXT NOT NULL,
    Tipo TEXT NOT NULL DEFAULT 'Contenção',
    Descricao TEXT NOT NULL,
    ResponsavelId TEXT,
    Departamento TEXT,
    Prazo TEXT,
    Status TEXT DEFAULT 'Pendente',
    DataConclusao TEXT,
    Observacoes TEXT,
    FOREIGN KEY (NonConformityId) REFERENCES NonConformities(Id) ON DELETE CASCADE,
    FOREIGN KEY (ResponsavelId) REFERENCES AspNetUsers(Id)
);

CREATE INDEX IF NOT EXISTS IX_NonConformityEvidences_NonConformityId ON NonConformityEvidences(NonConformityId);
CREATE INDEX IF NOT EXISTS IX_NonConformityActions_NonConformityId ON NonConformityActions(NonConformityId);
CREATE INDEX IF NOT EXISTS IX_NonConformities_Numero ON NonConformities(Numero);
CREATE INDEX IF NOT EXISTS IX_NonConformities_Status ON NonConformities(Status);
CREATE INDEX IF NOT EXISTS IX_NonConformities_EtapaAtual ON NonConformities(EtapaAtual);
";

var tableCommands = createTables.Split(';', StringSplitOptions.RemoveEmptyEntries);

foreach (var cmd in tableCommands)
{
    if (!string.IsNullOrWhiteSpace(cmd))
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = cmd.Trim();
            command.ExecuteNonQuery();
            Console.WriteLine($"✓ Executado: {cmd.Trim().Substring(0, Math.Min(50, cmd.Trim().Length))}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
        }
    }
}

Console.WriteLine("\n✅ Migração concluída!");
