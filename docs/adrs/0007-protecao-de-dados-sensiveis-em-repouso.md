# Proteção de dados sensíveis em repouso

> **Decisão arquitetural** · Público: arquitetura, segurança, dados e operação · Responsável: Engenharia · Última validação: 2026-08-13.

- Estado: aceita.
- Data: 2026-08-13.

## Contexto

O SQLite local pode conter sessão do equipamento, biometria, cartões, QR Codes,
fotos, configurações e cargas de Monitor, Push e logs. O SQLite convencional não
criptografa o arquivo. A criptografia transparente completa exigiria uma
distribuição nativa de SQLite com suporte criptográfico e gestão própria de
licença, build e atualização.

## Decisão

1. Proteger, por coluna, os valores sensíveis com ASP.NET Data Protection e
   finalidades criptográficas distintas.
2. Persistir o chaveiro fora do diretório efêmero e protegê-lo com certificado
   PKCS#12 fora de `Development`.
3. Exigir atestado explícito de volume criptografado fora de `Development`, pois
   metadados, índices, identificadores e o próprio arquivo SQLite não ficam
   integralmente cifrados pela proteção de colunas.
4. Não converter silenciosamente valores legados. A conversão exige cópia de
   segurança protegida, teste de restauração e confirmação explícita por
   `tools/protect-sensitive-sqlite-data.ps1`.
5. Tornar readiness não saudável e impedir startup não local enquanto houver
   valores legados em texto puro quando a proteção for obrigatória.

## Alternativas consideradas

- SQLCipher gratuito legado: rejeitado porque o pacote disponível foi
  descontinuado e introduziria dependência nativa sem manutenção compatível com
  a linha de base atual.
- Criptografia somente do volume: insuficiente isoladamente para cópias do
  arquivo e defesa em profundidade.
- Criptografia de todas as colunas: rejeitada porque quebraria filtros, índices e
  unicidade sem um desenho de busca criptográfica.

## Consequências

- Backups e restaurações precisam incluir SQLite, chaveiro, certificado e acesso
  à senha do certificado.
- Perder o chaveiro ou certificado torna os campos protegidos irrecuperáveis.
- Consultas diretas ao SQLite verão valores com prefixo `dp:v1:`.
- A aplicação continua dependendo de ACL, volume criptografado, retenção e
  controle de acesso para os campos não protegidos e metadados.
- Rotação criptográfica requer procedimento operacional e teste de restauração.

## Validação

```powershell
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --filter FullyQualifiedName~SensitiveDataProtectionStoreTests
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite-operational.ps1 -RunRestoreSmoke
```

Voltar ao [índice de decisões](README.md).
