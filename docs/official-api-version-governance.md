# Governança de versões da Access API

> **Documento vivo** · Público: manutenção, integração, QA e liberação · Responsável: engenharia de integração · Última validação: 2026-08-03.

Esta política mantém a documentação da PoC alinhada a uma API oficial viva. Uma
página oficial, endpoint no catálogo ou teste com simulador não substitui homologação
no produto e firmware usados pelo ambiente.

## Fontes canônicas

Use as fontes nesta ordem:

1. resposta observada no equipamento físico autorizado;
2. documentação oficial da Access API para o endpoint;
3. notas oficiais de firmware da linha e manual do produto;
4. catálogo e código atuais da PoC;
5. testes automatizados e simulador (stub);
6. relatórios históricos do repositório.

Uma evidência inferior não deve contradizer silenciosamente uma superior. Quando
houver divergência, registre-a e mantenha a capacidade como não homologada.

## Referências oficiais acompanhadas

| Área | Referência |
| --- | --- |
| Visão geral | [Introdução à Access API](https://www.controlid.com.br/docs/access-api-pt/) |
| Sessão | [Fazer login](https://www.controlid.com.br/docs/access-api-pt/gerenciamento-secao/fazer-login/) |
| Objetos | [Lista de objetos](https://www.controlid.com.br/docs/access-api-pt/objetos/lista-de-objetos/) |
| Regras | [Cadastrar usuários e regras](https://www.controlid.com.br/docs/access-api-pt/primeiros-passos/cadastrar-usuarios-e-suas-regras/) |
| Modos | [Introdução aos modos](https://www.controlid.com.br/docs/access-api-pt/modos-de-operacao/introducao-aos-modos-de-operacao/) |
| Monitor | [Introdução ao Monitor](https://www.controlid.com.br/docs/access-api-pt/monitor/introducao-ao-monitor/) |
| Push | [Introdução ao Push](https://www.controlid.com.br/docs/access-api-pt/modo-push/introducao-ao-push/) |
| Configuração | [Parâmetros de configuração](https://www.controlid.com.br/docs/access-api-pt/configuracoes/parametros-configuracao/) |
| Produtos | [Particularidades dos terminais](https://www.controlid.com.br/docs/access-api-pt/particularidade-dos-produtos/particulariade-terminais-control-id/) |
| Segurança | [Guia de fortalecimento](https://www.controlid.com.br/docs/access-api-en/system/security-hardening/) |
| Mudanças | [Notas de firmware em português](https://www.controlid.com.br/access_v2/changelog_pt-br.pdf) |

Os links foram verificados na revisão indicada nos metadados. Disponibilidade de
URL não comprova que o conteúdo permaneceu semanticamente igual.

## Identificação de uma capacidade

Cada capacidade documentada deve registrar:

| Campo | Conteúdo |
| --- | --- |
| Identificador | ID estável da PoC ou endpoint/caminho oficial. |
| Produto | Linha e variante exatas. |
| Firmware | Versão observada. |
| Licença | Classe/modo, nunca chave ou senha. |
| Direção | PoC → equipamento ou equipamento → PoC. |
| Contrato | Método, caminho, sessão, corpo, resposta e efeito. |
| Evidência | Teste, simulador, relatório físico ou página oficial. |
| Estado | Implementado, validado com simulador, homologado, parcial, não aplicável ou divergente. |
| Data/dono | Data da verificação e responsável. |

## Cadência

- A cada mudança no catálogo oficial da PoC: revisar endpoint e links afetados.
- A cada atualização de firmware: revalidar as capacidades tocadas pelas notas de
  versão e pelo teste de regressão.
- Antes de uma liberação operacional: conferir documentação oficial, matriz física e
  `ops.local.json`.
- Trimestralmente em projeto ativo: verificar URLs, notas de firmware e riscos
  pendentes.
- Após incidente: revalidar o contrato associado antes de encerrar a ação
  corretiva.

## Processo de revisão

1. Registrar commit e data de corte.
2. Exportar as 96 entradas atuais do catálogo e separar chamadas de callbacks.
3. Comparar método, caminho, sessão, direção, corpo e conteúdo esperado com a fonte
   oficial.
4. Identificar particularidades por produto, firmware e licença.
5. Executar testes unitários e contrato com o simulador (stub).
6. Executar `contract-controlid-device.ps1` para leitura/sessão quando autorizado.
7. Homologar efeitos físicos separadamente, com retorno aprovado.
8. Atualizar `device-compatibility-matrix.md` e os documentos de contrato.
9. Registrar divergências e risco residual; não alterar histórico silenciosamente.

## Tratamento de mudanças

| Tipo | Ação |
| --- | --- |
| Novo endpoint oficial | Confirmar aplicabilidade, adicionar contrato e teste antes de anunciar suporte. |
| Campo novo opcional | Tornar o analisador tolerante somente com evidência e teste. |
| Campo removido/renomeado | Tratar como possível quebra por firmware; preservar contrato público da PoC até plano versionado. |
| Mudança de semântica | Registrar ADR/impacto, teste físico e migração de documentação. |
| Recurso exclusivo | Marcar produto/firmware/licença; não exibir como universal. |
| URL oficial movida | Atualizar link sem reescrever resultado histórico. |
| Firmware com correção | Reexecutar apenas fluxos afetados e regressão mínima crítica. |

## Registros históricos

`reports/controlid-api-audit-2026-04-13.md` e os relatórios de homologação de
2026-04-14 preservam o corte observado nessas datas. Eles não devem receber
afirmações retrospectivas sobre firmware posterior. Uma nova rodada cria novo
relatório datado ou atualiza somente documentos vivos.

## Critério para declarar suporte

- **Implementado:** o software possui caminho funcional e testes locais.
- **Compatível com contrato:** simulador ou teste de integração confirmou o HTTP
  esperado.
- **Homologado:** equipamento, firmware, licença, topologia e efeito foram
  validados e registrados.
- **Pronto para produção:** além da homologação, verificações operacionais, segurança,
  privacidade, backup, observabilidade e aprovação humana foram concluídos.

Não use “suportado” sem qualificador quando apenas a primeira ou segunda camada
foi comprovada.

## Validações

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1 -CheckExternalUrls
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-stub.ps1
```

A verificação externa mede a disponibilidade dos links, não a equivalência semântica. A
revisão humana deve comparar conteúdo, exemplos e particularidades.
