# Changelog técnico - 2026-04-15

## O que mudou

- O README foi atualizado com setup mais guiado, uso de User Secrets, variáveis de ambiente detalhadas, execução de testes e checklist de observabilidade.
- A documentação funcional foi reforçada com guias dedicados para modos de operação, Monitor e Push.
- Foram adicionados comentários XML em métodos críticos de modos de operação, Push, callbacks e invocação oficial da API.
- O fluxo de modos de operação passou a registrar logs estratégicos para aplicação de Standalone, Pro, Enterprise, upgrades, validação de sessão e resolução de `server_id`.
- A central Push passou a registrar sinais mais úteis para troubleshooting, incluindo polling, payload entregue, resultados sem `command_id`, comandos inexistentes e falhas de persistência.
- A camada de callbacks recebeu documentação inline para leitura de corpo, validação de segurança, limite de payload, IP permitido e chave compartilhada.
- O README agora lista sinais práticos para monitoramento local ou integração futura com ferramentas externas.

## Por que mudou

- Facilitar a entrada de desenvolvedores que ainda não conhecem a PoC, reduzindo dependência de contexto informal.
- Tornar a operação local mais previsível, especialmente em cenários com equipamento real, callbacks, Push e modos online.
- Aumentar a capacidade de diagnóstico por console ou arquivo de log sem expor segredos, licenças ou payloads sensíveis.
- Deixar claro quais variáveis de ambiente controlam conexão, sessão, callbacks, segurança e logs.
- Preparar a PoC para evoluções futuras de monitoramento, como alertas, dashboards, coleta externa de logs ou SignalR.

## Impacto esperado

- Menor tempo para configurar a PoC em uma máquina nova.
- Mais clareza para debugar falhas de API, callbacks, Monitor, Push e modos de operação.
- Melhor rastreabilidade de transições entre Standalone, Pro e Enterprise.
- Documentação mais útil para revisão em Pull Request e para uso público do repositório como referência.

## Observações para revisão

- Nenhum segredo, senha ou licença deve ser versionado.
- Os logs adicionados evitam registrar valores sensíveis e priorizam metadados operacionais.
- Os testes automatizados devem ser executados com `dotnet test .\Integracao.ControlID.PoC.sln`.
- O smoke test local continua disponível em `tools/smoke-localhost.ps1`.

