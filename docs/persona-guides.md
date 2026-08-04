# Percursos por perfil de usuário

> **Documento vivo** · Público: primeiro contato, produto, integração e operação · Responsável: produto técnico · Última validação: 2026-08-03.

Este documento oferece percursos curtos para públicos diferentes. Cada percurso
termina em uma evidência verificável e evita conceder permissões ou coletar dados
além do necessário.

## Escolha seu percurso

| Perfil | Pergunta principal | Comece por | Resultado esperado |
| --- | --- | --- | --- |
| Avaliador sem hardware | “O que a PoC demonstra?” | `README.md` e `faq.md` | Fluxo com simulador concluído. |
| Desenvolvedor integrador | “Como chamar a Access API corretamente?” | `developer-onboarding.md` e `integration-contracts.md` | Contrato com simulador e testes aprovados. |
| Administrador de acesso | “Como cadastrar e operar identidades?” | `local-account-administration.md` e `data-synchronization-ownership.md` | Fluxo de usuário/regra entendido e ensaiado. |
| Infraestrutura/rede | “Quem precisa alcançar quem?” | `network-topologies.md` | Matriz de fluxos e portas aprovada. |
| Segurança/DPO | “Quais dados e fronteiras precisam de controle?” | `security-hardening.md` e `privacy-and-data-retention.md` | Controles e aprovações externas identificados. |
| QA/homologação | “O que está comprovado?” | `testing-strategy.md` e `device-compatibility-matrix.md` | Evidência separa simulador de hardware. |
| Suporte/SRE | “Como diagnosticar sem vazar dados?” | `troubleshooting-controlid.md` | Incidente classificado e evidência segura. |
| Responsável por produção | “A solução pode ser liberada?” | `residual-risk-closure.md` e `deployment-runbook.md` | Verificação estrita e aprovações avaliadas. |

## Avaliador sem equipamento físico

Objetivo: compreender valor e limites sem tocar um ambiente real.

1. Leia a visão geral e os limites no `README.md`.
2. Crie uma conta local com dados fictícios em `/Auth/Register`.
3. Execute `tools/ControlIdDeviceStub` e conecte `http://127.0.0.1:6600`.
4. Faça login no simulador com as credenciais fictícias documentadas.
5. Consulte `system_information.fcgi`, catálogo, eventos e Push.
6. Execute `tools/smoke-localhost.ps1`.

Não conclua que câmera, biometria, relé, licença ou firmware reais foram
homologados. Evidência de conclusão: teste integrado aprovado e limitações registradas.

## Desenvolvedor de integração

Objetivo: implementar ou validar um contrato sem quebrar endpoints existentes.

1. Identifique o fluxo em `product-acceptance-criteria.md`.
2. Confirme método, caminho, sessão, corpo e resposta em
   `OfficialApiCatalogService` e na documentação oficial.
3. Classifique a direção: PoC para equipamento ou equipamento para PoC.
4. Use DTO/analisador estruturado e mantenha tempo limite/cancelamento.
5. Não adicione retentativa para escrita sem idempotência.
6. Crie teste unitário/contrato e execute o simulador.
7. Atualize `integration-contracts.md` e a matriz de compatibilidade.

Evidência de conclusão: compilação, testes, formatação e contrato simulado aprovados.

## Administrador de controle de acesso

Objetivo: entender usuários, credenciais e regras antes de qualquer escrita.

1. Diferencie a conta local da PoC do usuário cadastrado no equipamento.
2. Confirme modelo, firmware, licença e modo.
3. Em Standalone, planeje usuários, grupos, regras, horários e portais.
4. Cadastre com dados fictícios em bancada antes de usar dados autorizados.
5. Releia cada objeto e teste acesso em área controlada.
6. Não execute exclusão, reset ou remoção de administradores sem backup e
   confirmação.

Evidência de conclusão: objetos e vínculos relidos, acesso físico observado e
retorno documentado.

## Infraestrutura e rede

Objetivo: fornecer conectividade mínima e segura.

1. Desenhe os fluxos de `network-topologies.md`.
2. Liste origem, destino, protocolo, porta, DNS e responsável.
3. Defina se o equipamento iniciará Monitor, Push ou chamadas on-line.
4. Prefira rede privada/VPN e TLS; evite exposição direta.
5. Configure listas de permissões, proxy conhecido e HMAC/proxy assinador.
6. Teste ida, retorno, tempo limite e contingência.

Evidência de conclusão: matriz de firewall aprovada, verificações de saúde bem-sucedidas e
callback fictício recebido.

## Segurança e privacidade

Objetivo: impedir exposição de credenciais e dados de controle de acesso.

1. Classifique contas, fotos, biometria, cartões, QR Codes e logs.
2. Confirme menor privilégio local e no equipamento.
3. Mantenha segredos em cofre/User Secrets/variáveis protegidas.
4. Exija HMAC, timestamp, nonce e IP permitido fora de Development.
5. Defina retenção, descarte, DSAR e RIPD com DPO/jurídico.
6. Execute a varredura de segredos e revise os registros.

Evidência de conclusão: controles técnicos aprovados e decisões jurídicas
marcadas como externas, nunca presumidas.

## QA e homologação física

Objetivo: distinguir contrato de software de efeito no equipamento.

1. Execute testes unitários e contrato com o simulador (stub).
2. Selecione uma célula da `device-compatibility-matrix.md`.
3. Registre modelo, firmware, licença, modo e estado anterior.
4. Execute somente o caso aprovado, com dados fictícios/minimizados.
5. Valide resposta, releitura, callback e efeito físico.
6. Restaure a configuração anterior e registre resultado.

Evidência de conclusão: relatório sanitizado por célula, com aprovado, parcial,
reprovado ou não aplicável.

## Suporte e SRE

Objetivo: restaurar serviço e manter rastreabilidade.

1. Classifique severidade em `incident-response-and-dr.md`.
2. Confirme a saúde da aplicação, a sessão, a direção da chamada e as dependências.
3. Capture `X-Correlation-ID`, status e duração.
4. Use `troubleshooting-controlid.md` e `api-error-catalog.md`.
5. Aplique contenção e contingência aprovadas.
6. Revalide fluxo, dados e logs após a recuperação.

Evidência de conclusão: serviço normalizado, dados reconciliados e ação corretiva
com responsável.

## Responsável pela liberação

Objetivo: decidir pela liberação ou não liberação sem transformar ausência de evidência em aprovação.

1. Preencha `ops.local.json` fora do Git.
2. Confirme cópia de segurança externa, restauração, RTO/RPO, DNS/TLS e responsáveis.
3. Execute `tools/test-readiness-gates.ps1 -ReleaseGate`.
4. Exija contrato físico e analisadores externos.
5. Verifique LGPD/DPO, FinOps, alertas, procedimentos operacionais e reversão.
6. Registre riscos aceitos e aprovação humana.

Evidência de conclusão: verificação estrita aprovada e lista de produção assinada.

## Percursos por objetivo

| Objetivo | Sequência mínima |
| --- | --- |
| Demonstrar sem hardware | README → conta local → simulador → login do simulador → catálogo → teste integrado. |
| Consultar equipamento real | Conta local → rede → conexão → login oficial → leitura → logout. |
| Receber eventos | Topologia → segurança de ingresso → Monitor/callback → persistência → observabilidade. |
| Operar Push | Configurar servidor Push → enfileirar → consulta periódica → resultado → reconciliação. |
| Alterar modo | Compatibilidade → cópia de segurança/configuração anterior → `server_id` → perfil → releitura → callback. |
| Preparar produção | Segurança → privacidade → infraestrutura → recuperação de desastres → verificação de liberação → aprovação humana. |

Para perguntas pontuais, use `faq.md`; para detalhes de arquivos e comandos, use
`docs/README.md` e `project-file-responsibilities.md`.
