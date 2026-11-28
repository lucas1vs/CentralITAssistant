# CentralITAssistant — Lucia

**Assistente Inteligente da Central IT**

O **CentralITAssistant**, também chamado de **Lucia**, é um projeto de assistente virtual inteligente desenvolvido com o objetivo de facilitar o atendimento interno de TI, oferecendo aos colaboradores um canal de comunicação para consultar informações, esclarecer dúvidas e obter orientações sobre procedimentos e serviços de tecnologia.

A proposta é centralizar o atendimento e tornar a comunicação entre os colaboradores e a equipe de TI mais ágil, acessível e eficiente.

---

## Objetivos do projeto

- Facilitar o atendimento de solicitações internas de TI.
- Permitir que colaboradores consultem informações por meio de linguagem natural.
- Automatizar etapas de identificação e validação de colaboradores.
- Implementar autenticação por código de verificação enviado por e-mail corporativo.
- Integrar um modelo de inteligência artificial para interpretação e geração de respostas.
- Preparar a integração com o WhatsApp como canal de atendimento.
- Estruturar uma arquitetura escalável para futuras funcionalidades.

## Arquitetura

O projeto utiliza **Clean Architecture**, organizando as responsabilidades em camadas independentes para facilitar manutenção, testes e evolução.

```text
CentralITAssistant/
│
├── src/
│   ├── CentralITAssistant.Domain/
│   │
│   ├── CentralITAssistant.Application/
│   │
│   ├── CentralITAssistant.Infrastructure/
│   │
│   └── CentralITAssistant.API/
│
├── tests/
│   └── CentralITAssistant.Tests/
│
├── CentralITAssistant.sln
└── README.md
```

### Responsabilidades das camadas

| Camada | Responsabilidade |
|---|---|
| Domain | Entidades e regras de negócio centrais. |
| Application | Casos de uso, contratos e abstrações. |
| Infrastructure | Persistência, serviços externos e integrações. |
| API | Endpoints, configuração e exposição dos serviços. |
| Tests | Testes automatizados da aplicação. |

## Tecnologias utilizadas

- **C#** — linguagem principal.
- **.NET 9** — plataforma de desenvolvimento.
- **ASP.NET Core** — construção da API.
- **Entity Framework Core 9** — ORM e persistência.
- **SQL Server** — banco de dados principal.
- **SQLite** — suporte aos testes automatizados.
- **xUnit** — testes unitários.
- **Swashbuckle / Swagger** — documentação e exploração da API.
- **Meta Model API** — integração com modelo de inteligência artificial.
- **Muse Spark 1.3** — modelo configurado para geração de respostas.
- **HttpClient** — comunicação com serviços externos.
- **HMAC-SHA256** — proteção dos códigos de verificação.
- **Git e GitHub** — versionamento e gerenciamento do código.
- **Docker** — previsto para a etapa de containerização.

## Inteligência artificial

A Lucia possui uma estrutura de integração com a **Meta Model API**, utilizando o modelo **Muse Spark 1.3** por meio da Responses API.

A integração foi desenvolvida com:

- Abstração `IAiService` na camada Application.
- Implementação `MetaAiService` na camada Infrastructure.
- Comunicação HTTP utilizando `HttpClient`.
- Configuração do modelo e da chave de API por User Secrets.
- Registro de dependência na API.
- Testes unitários utilizando um `HttpMessageHandler` simulado.
- Teste de integração em ambiente real.

**Status da integração:** implementação e testes unitários concluídos. A validação com a API real permanece pendente devido a uma resposta HTTP 402 (Payment Required), sendo necessário verificar as condições de acesso e faturamento da plataforma.

A chave de API não deve ser armazenada no repositório.

## Autenticação e segurança

O projeto contempla uma estrutura de validação de colaboradores por e-mail corporativo.

O fluxo planejado utiliza códigos OTP (*One-Time Password*) com as seguintes regras:

| Parâmetro | Configuração |
|---|---|
| Tamanho do código | 6 dígitos |
| Validade | 5 minutos |
| Tentativas máximas | 5 |
| Intervalo de reenvio | 60 segundos |
| Reenvios máximos | 5 |
| Proteção | HMAC-SHA256 |

O envio de e-mails é abstraído por meio da interface `IEmailService`, permitindo que o provedor seja implementado ou substituído sem acoplar a lógica de negócio a uma tecnologia específica.

As credenciais e informações sensíveis devem permanecer fora do controle de versão.

## Banco de dados

A persistência utiliza Entity Framework Core com abordagem Code First e SQL Server.

As entidades iniciais do domínio incluem:

- `Employee`
- `WhatsAppContact`
- `Conversation`

As alterações de estrutura do banco são controladas por migrations do Entity Framework Core.

## Integrações e canais de atendimento

### WhatsApp

O WhatsApp é o canal de atendimento previsto para a Lucia, com integração planejada por meio da Meta WhatsApp Cloud API e utilização de webhooks para recebimento de mensagens.

### E-mail corporativo

O Zimbra foi considerado como referência para o fluxo de verificação de e-mail corporativo.

### Base de conhecimento

O CITSmart é considerado uma referência inicial para informações e procedimentos de TI, sem integração direta por API estabelecida até o momento.

### Telegram

O Telegram foi utilizado temporariamente como alternativa para testes de fluxos durante o desenvolvimento.

## Testes automatizados

O projeto utiliza xUnit para validar regras de negócio, serviços e integrações.

Os testes incluem:

- Validação das entidades do domínio.
- Regras de negócio.
- Comportamento dos serviços.
- Tratamento de respostas HTTP.
- Testes da integração com o serviço de IA utilizando respostas simuladas.
- Teste opcional de integração real com a Meta Model API.

Para executar a suíte de testes:

```powershell
dotnet test
```

Para executar somente os testes da integração real com a Meta Model API, configure a variável de ambiente:

```powershell
$env:RUN_META_LIVE_TESTS = "true"
dotnet test
```

A execução do teste real depende de uma chave válida e de acesso habilitado à plataforma.

## Configuração do ambiente

### Pré-requisitos

- .NET SDK 9.
- SQL Server.
- Visual Studio ou Visual Studio Code.
- Git.
- Acesso à Meta Model API para executar a integração real.

### Configuração da Meta Model API

As configurações devem ser armazenadas utilizando .NET User Secrets:

```powershell
dotnet user-secrets set "Meta:ApiKey" "SUA_CHAVE" --project .\src\CentralITAssistant.API

dotnet user-secrets set "Meta:Model" "muse-spark-1.3" --project .\src\CentralITAssistant.API
```

Não compartilhe sua chave de API e não a inclua em arquivos versionados.

### Executando a API

Na raiz da solução:

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\CentralITAssistant.API
```

A documentação da API pode ser acessada pelo Swagger, conforme a configuração do ambiente.

## Próximas etapas

- [x] Estrutura inicial da solução.
- [x] Implementação das primeiras entidades do domínio.
- [x] Criação dos testes automatizados iniciais.
- [x] Configuração do Entity Framework Core.
- [x] Estruturação do fluxo de autenticação por e-mail.
- [x] Criação da abstração do serviço de inteligência artificial.
- [x] Implementação da integração com a Meta Model API.
- [x] Criação dos testes unitários da integração com IA.
- [ ] Validar a integração real com a Meta Model API.
- [ ] Evoluir a integração com o WhatsApp Cloud API.
- [ ] Implementar os webhooks de recebimento de mensagens.
- [ ] Evoluir a integração com o Zimbra.
- [ ] Desenvolver a integração com a base de conhecimento.
- [ ] Aprimorar o fluxo de conversação e interpretação de intenções.
- [ ] Preparar a aplicação para execução com Docker.
- [ ] Realizar deploy e validação final.

## Versionamento

O projeto utiliza Git e GitHub para controle de versão.

**Repositório:** [CentralITAssistant](https://github.com/lucas1vs/CentralITAssistant)

---

**CentralITAssistant — Lucia**  
*Inteligência artificial aplicada ao atendimento interno de TI.*
