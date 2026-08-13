# Matriz de validação dos endpoints

> **Referência** · Público: produto, integração, QA e release · Responsável: Engenharia · Última validação: 2026-08-12.

Esta matriz separa implementação, simulação e homologação. O catálogo executável
em `OfficialApiDocumentationSeedCatalog` continua sendo a fonte dos paths,
métodos, corpos e requisitos de sessão.

## Níveis de evidência

| Nível | Significado |
| --- | --- |
| `CÓDIGO` | Rota e transformação existem e compilam |
| `TESTE` | Regra ou contrato tem teste automatizado isolado |
| `STUB` | Jornada HTTP foi executada contra o simulador |
| `E2E` | Interface autenticada executou a família no navegador |
| `FÍSICO` | Equipamento, firmware e licença foram registrados em homologação |

## Cobertura por família

| Família | Exemplos oficiais | Evidência sem hardware | Evidência física necessária |
| --- | --- | --- | --- |
| Sessão | `login.fcgi`, `logout.fcgi`, `session_is_valid.fcgi` | CÓDIGO, TESTE, STUB, E2E | Expiração e limite do firmware |
| Sistema | `system_information.fcgi`, data/hora, hash e credenciais | CÓDIGO, TESTE, STUB, E2E | Relógio, reboot e efeito real |
| Objetos | `load_objects.fcgi`, `create_objects.fcgi`, `modify_objects.fcgi`, `destroy_objects.fcgi` | CÓDIGO, TESTE, STUB, E2E | Campos por firmware e atomicidade |
| Usuários e credenciais | usuários, cartões, QR Codes, biometria e fotos | CÓDIGO, TESTE, STUB parcial, E2E de consulta | Leitores, imagem, biometria e retenção |
| Acesso | regras, grupos, áreas, portais, horários e ações remotas | CÓDIGO, TESTE, STUB parcial | Relé, porta, catraca e anti-passback |
| Monitor/callback | monitor, identificação, autorização e eventos | CÓDIGO, TESTE, STUB, smoke | Origem do firmware e topologia real |
| Push | `/push`, `/result` e recepção local | CÓDIGO, TESTE, STUB, smoke | Polling e idempotência do aparelho |
| Configuração | rede, VPN, SSL, modo on-line e iDCloud | CÓDIGO, TESTE, STUB parcial | Rede, certificado e serviço externo |
| Mídia | logo, foto, áudio, vídeo e screenshot | CÓDIGO, TESTE, STUB parcial, E2E de tela | Codec, display, câmera e áudio |
| Produto/licença | iDFace, iDFlex, iDBox, Pro e Enterprise | CÓDIGO, TESTE, perfis STUB | Modelo, firmware e licença específicos |
| Alto impacto | reset, recovery, remoção de admins e reboot | CÓDIGO e TESTE de bloqueio | Execução manual aprovada e rollback |

## Regras de atualização

1. Não promover `STUB` para `FÍSICO` por inferência.
2. Registrar no mínimo modelo, serial pseudonimizado, firmware, licença, data,
   topologia, endpoint, entrada sanitizada, status e efeito observado.
3. Operações destrutivas exigem confirmação humana e ambiente descartável.
4. Resposta divergente deve atualizar contrato, fixture e teste somente após
   confirmar documentação/firmware; não adapte silenciosamente a regra.
5. Dado pessoal ou biométrico real não entra no repositório nem no relatório.

## Lacunas físicas atuais

Nenhuma família possui declaração universal de homologação física. O limite está
registrado em [docs/integracao-controlid/device-compatibility-matrix.md](device-compatibility-matrix.md) e
[docs/operacao/residual-risk-closure.md](../operacao/residual-risk-closure.md). A PoC está pronta para levar a execução até o
aparelho, mas compatibilidade definitiva continua condicionada ao equipamento.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
