# Administração de contas locais

> **Documento vivo** · Público: usuários da PoC, administradores e suporte · Responsável: segurança da aplicação · Última validação: 2026-08-03.

Este documento explica por que a PoC possui contas próprias, como elas se
relacionam com as credenciais do equipamento Control iD e quais operações de
ciclo de vida estão realmente implementadas. Ele não descreve um provedor de
identidade corporativo, porque a aplicação não possui SSO, diretório externo ou
MFA integrados.

## Resposta direta

Uma conta local é necessária para o uso humano normal das telas MVC/Razor. A
aplicação aplica uma política global que exige usuário autenticado, com exceções
explícitas para cadastro inicial, login, páginas de erro, verificações de saúde e
rotas máquina a máquina protegidas por controles próprios.

A conta local protege a PoC; ela não autentica o usuário no equipamento. Para
consultar ou alterar um Control iD, também é necessário conectar o equipamento e
executar o login oficial da Access API. São duas fronteiras independentes:

| Camada | Credencial | Estado criado | Finalidade |
| --- | --- | --- | --- |
| PoC | Usuário/e-mail e senha locais | Cookie `.IntegracaoControlID.Auth` | Identificar a pessoa e aplicar os papéis locais. |
| Equipamento | Usuário e senha do Control iD | `session` oficial guardada na sessão ASP.NET | Autorizar chamadas `.fcgi` no equipamento selecionado. |

O encerramento do login local limpa o cookie e o estado ASP.NET da navegação. O
logout do equipamento chama `logout.fcgi` e remove somente a sessão oficial. Não
trate essas duas ações como equivalentes.

## Primeiro administrador

1. Quando a tabela local `Users` está vazia, `/Auth/Register` permite o primeiro
   cadastro.
2. O cadastro ocorre de forma transacional no SQLite para evitar dois primeiros
   administradores concorrentes.
3. O primeiro usuário recebe o papel `Administrator`.
4. Depois do primeiro cadastro, somente um administrador autenticado consegue
   abrir novamente `/Auth/Register`.
5. Cadastros posteriores recebem o papel `Operator`.

Em demonstrações, use somente nomes e e-mails fictícios. A senha deve ter de 12
a 128 caracteres. O repositório não fornece senha pronta para a conta local.

## Papéis e permissões atuais

| Capacidade | `Operator` | `Administrator` | Evidência de implementação |
| --- | --- | --- | --- |
| Entrar e sair da PoC | Sim | Sim | `AuthController.LocalLogin` e `LocalLogout`. |
| Alterar a própria senha conhecendo a atual | Sim | Sim | `AuthController.ChangePassword`. |
| Navegar pelo painel, mapa funcional e catálogo | Sim | Sim | Política global de usuário autenticado. |
| Testar conexão e colocar um equipamento no contexto da sessão | Sim | Sim | `HomeController.ConnectToDevice` e `TestDeviceConnectivity`. |
| Fazer login/logout oficial no equipamento | Sim | Sim | `AuthController.Login` e `Logout`. |
| Invocar um endpoint oficial por POST | Não | Sim | `OfficialApiController.Invoke` exige `Administrator`. |
| Gerenciar usuários, grupos, cartões, regras, hardware e configurações | Não | Sim | Controllers dessas áreas exigem `Administrator`. |
| Executar ações físicas ou destrutivas | Não | Sim, com confirmação quando aplicável | Controllers administrativos e `HighImpactOperationGuard`. |
| Consultar detalhes de eventos/Push e limpar históricos | Não | Sim | Ações protegidas nos controllers de eventos e Push. |
| Criar outra conta local | Não | Sim | `CanRegisterLocalUserAsync`. |
| Consultar `/metrics` na configuração padrão | Não | Sim | Política `AdministratorOnly`. |

O papel `Operator` é deliberadamente limitado. Ele permite diagnóstico inicial,
navegação e criação de uma sessão com o equipamento, mas não concede escrita
administrativa nem autorização para operações físicas. A interface pode exibir
links que resultarão em acesso negado; a decisão confiável é sempre a autorização
no controller.

## Sessões e expiração

| Estado | Padrão | Configuração |
| --- | ---: | --- |
| Cookie de autenticação local | 60 minutos com renovação deslizante | `Auth:IdleTimeoutMinutes`, limitado entre 5 e 1440 minutos. |
| Sessão ASP.NET que guarda equipamento e `session` oficial | 30 minutos de inatividade | `Session:IdleTimeout`. |
| Sessão oficial Control iD | Definida pelo equipamento/firmware | Validar com `session_is_valid.fcgi`. |

O recurso “Lembrar-me” torna o cookie local persistente, mas não transforma a
sessão Control iD em permanente. Uma tela indicar “sessão ativa” significa apenas
que existe um valor no estado ASP.NET; antes de uma operação sensível, valide a
sessão no equipamento.

## Alteração de senha

- O próprio usuário pode alterar sua senha ao informar usuário, senha atual e
  nova senha.
- Um administrador pode abrir o mesmo fluxo para outro identificador, mas a
  implementação ainda exige a senha atual dessa conta.
- A senha é armazenada como hash PBKDF2; senha em claro não é persistida nem deve
  aparecer em registros.
- Hashes legados reconhecidos são atualizados para PBKDF2 após login válido.

## Capacidades que não existem

No estado atual, a PoC não possui:

- recuperação por e-mail ou link de redefinição;
- redefinição administrativa sem a senha atual;
- tela para promover, rebaixar, desativar ou excluir contas locais;
- MFA, SSO, OIDC, SAML ou integração com Active Directory;
- bloqueio persistente de conta após tentativas inválidas;
- gestão de sessões simultâneas por usuário.

A limitação de taxa reduz tentativas automatizadas, mas não substitui bloqueio de
conta, MFA ou um provedor de identidade.

## Recuperação quando o administrador perde acesso

Não há fluxo suportado na interface para recuperar a única conta administrativa
sem a senha atual. Trate o cenário como incidente operacional:

1. preserve o SQLite, os arquivos `-wal`/`-shm` e os registros; não edite o banco
   manualmente durante o diagnóstico;
2. valide uma cópia conhecida com `tools/restore-smoke-sqlite.ps1`;
3. restaure uma cópia anterior somente com aprovação humana e seguindo
   `data-model-and-recovery.md`;
4. se não houver cópia válida, registre uma decisão técnica para recuperação do
   banco ou recriação de ambiente, com cópia prévia, evidência e revisão de
   segurança;
5. nunca contorne o login alterando hash, papel ou status diretamente sem um
   procedimento aprovado e auditável.

Em ambiente puramente descartável, recriar o SQLite elimina todo o estado local,
não apenas a conta. Isso é destrutivo e exige confirmação explícita.

## Procedimentos usuais

### Criar a primeira conta

1. Inicie a aplicação com banco novo ou validado.
2. Abra `/Auth/Register`.
3. Cadastre dados fictícios e uma senha dentro da política.
4. Entre em `/Auth/LocalLogin` e confirme o papel administrativo nas telas
   protegidas.

### Criar um operador

1. Entre com uma conta `Administrator`.
2. Abra `/Auth/Register`.
3. Cadastre o operador com dados mínimos.
4. Saia e teste a nova conta, confirmando que operações administrativas retornam
   acesso negado.

### Encerrar corretamente

1. Use o logout do equipamento para invalidar a sessão oficial quando ela ainda
   estiver disponível.
2. Use o logout local para encerrar a identidade na PoC e limpar o estado da
   navegação.
3. Em estação compartilhada, feche o navegador e não preserve credenciais.

## Verificações de segurança

- Não use a mesma senha na PoC e no equipamento.
- Não preencha credenciais reais em `appsettings*.json`, `.env.example`, testes ou
  documentos.
- Fora de `Development`, sirva a aplicação somente por HTTPS e mantenha cookies
  seguros.
- Revise contas e papéis diretamente no ambiente operacional restrito até existir
  uma tela administrativa segura.
- Investigue eventos repetidos de falha de login por referência pseudonimizada e
  identificador de correlação; não registre a senha nem o identificador integral.

## Validação relacionada

```powershell
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build -v:minimal
dotnet test .\tests\Integracao.ControlID.PoC.E2E\Integracao.ControlID.PoC.E2E.csproj --no-build -v:minimal
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

Consulte também `security-hardening.md`, `privacy-and-data-retention.md` e
`developer-onboarding.md`.
