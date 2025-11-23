-- Adicionar novas colunas à tabela NonConformities
ALTER TABLE NonConformities ADD COLUMN Titulo TEXT;
ALTER TABLE NonConformities ADD COLUMN Numero TEXT NOT NULL DEFAULT '';
ALTER TABLE NonConformities ADD COLUMN Severidade TEXT DEFAULT 'Média';
ALTER TABLE NonConformities ADD COLUMN Departamento TEXT;
ALTER TABLE NonConformities ADD COLUMN ClienteEnvolvidoId TEXT;
ALTER TABLE NonConformities ADD COLUMN TipoIncidente TEXT;
ALTER TABLE NonConformities ADD COLUMN NormaIsoId TEXT;
ALTER TABLE NonConformities ADD COLUMN PrazoConclusao TEXT;

-- Responsáveis
ALTER TABLE NonConformities ADD COLUMN ResponsavelContencaoId TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelAnaliseId TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelCorretivasId TEXT;
ALTER TABLE NonConformities ADD COLUMN ResponsavelVerificacaoId TEXT;

-- Ações de Contenção
ALTER TABLE NonConformities ADD COLUMN AcoesContencao TEXT;
ALTER TABLE NonConformities ADD COLUMN DataContencao TEXT;

-- Análise de Causa
ALTER TABLE NonConformities ADD COLUMN AnaliseCausa TEXT;
ALTER TABLE NonConformities ADD COLUMN CausaRaiz TEXT;
ALTER TABLE NonConformities ADD COLUMN DataAnalise TEXT;

-- Ações Corretivas
ALTER TABLE NonConformities ADD COLUMN AcoesCorretivas TEXT;
ALTER TABLE NonConformities ADD COLUMN DataCorretivas TEXT;

-- Verificação
ALTER TABLE NonConformities ADD COLUMN ResultadoVerificacao TEXT;
ALTER TABLE NonConformities ADD COLUMN DataVerificacao TEXT;
ALTER TABLE NonConformities ADD COLUMN EficazVerificacao INTEGER;

-- Controle
ALTER TABLE NonConformities ADD COLUMN Progresso INTEGER DEFAULT 0;

-- Criar tabela NonConformityEvidences
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

-- Criar tabela NonConformityActions
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

-- Criar índices para melhor performance
CREATE INDEX IF NOT EXISTS IX_NonConformityEvidences_NonConformityId ON NonConformityEvidences(NonConformityId);
CREATE INDEX IF NOT EXISTS IX_NonConformityActions_NonConformityId ON NonConformityActions(NonConformityId);
CREATE INDEX IF NOT EXISTS IX_NonConformities_Numero ON NonConformities(Numero);
CREATE INDEX IF NOT EXISTS IX_NonConformities_Status ON NonConformities(Status);
CREATE INDEX IF NOT EXISTS IX_NonConformities_EtapaAtual ON NonConformities(EtapaAtual);
