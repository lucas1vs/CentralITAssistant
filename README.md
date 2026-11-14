# CentralITAssistant

**Assistente inteligente para colaboradores da Central IT, acessível pelo WhatsApp.**

O CentralITAssistant é um projeto de desenvolvimento de um assistente virtual baseado em Inteligência Artificial, cujo objetivo é facilitar o acesso dos colaboradores a informações internas, procedimentos e orientações corporativas.

A proposta é disponibilizar um canal de comunicação simples e acessível, permitindo que os funcionários consultem informações e recebam notificações sobre reuniões, eventos e atividades relacionadas ao ambiente corporativo.

> **Status:** Em desenvolvimento — MVP.

## Objetivos do projeto

- Disponibilizar um assistente virtual acessível pelo WhatsApp.
- Responder dúvidas dos colaboradores com base em informações corporativas confiáveis.
- Centralizar orientações sobre serviços, procedimentos e processos internos.
- Enviar notificações sobre reuniões, eventos e atividades de onboarding.
- Implementar identificação e verificação de colaboradores por e-mail corporativo.
- Construir uma arquitetura preparada para futuras integrações com sistemas corporativos.

## Funcionalidades planejadas para o MVP

- [ ] Integração com WhatsApp.
- [ ] Identificação de colaboradores.
- [ ] Verificação de identidade por código OTP enviado ao e-mail corporativo.
- [ ] Atendimento automatizado com Inteligência Artificial.
- [ ] Consulta de informações e procedimentos internos.
- [ ] Notificações de reuniões e eventos.
- [ ] Integração futura com Microsoft Graph para recursos de calendário.

O CITSmart será utilizado inicialmente como fonte de referência para informações e procedimentos, sem depender de integração direta com sua API.

## Arquitetura

O projeto utiliza os princípios da **Clean Architecture**, mantendo as responsabilidades separadas entre as camadas.

```text
CentralITAssistant
│
├── src
│   ├── CentralITAssistant.API
│   │
│   ├── CentralITAssistant.Application
│   │
│   ├── CentralITAssistant.Domain
│   │
│   └── CentralITAssistant.Infrastructure
│
└── tests
    └── CentralITAssistant.Tests
```

### Responsabilidades

| Camada | Responsabilidade |
|---|---|
| API | Exposição dos endpoints e comunicação HTTP. |
| Application | Casos de uso, contratos e regras de aplicação. |
| Domain | Entidades e regras de negócio. |
| Infrastructure | Persistência, banco de dados e integrações externas. |
| Tests | Testes automatizados da aplicação. |

## Tecnologias utilizadas

- **C#**
- **.NET 9**
- **ASP.NET Core Web API**
- **Entity Framework Core 9**
- **SQL Server**
- **SQLite** para testes automatizados
- **xUnit**
- **Swagger / Swashbuckle**
- **Git e GitHub**
- **Docker** — previsto para evolução do ambiente.

## Banco de dados

A persistência utiliza Entity Framework Core com abordagem Code First e configurações Fluent API.

Entidades iniciais:

- `Employee`
- `WhatsAppContact`
- `Conversation`

As migrations são utilizadas para versionar a estrutura do banco de dados.

## Testes automatizados

O projeto utiliza xUnit para validar as regras de negócio e os componentes de persistência.

**Estado atual:** 48 testes automatizados aprovados.

Os testes de repositório utilizam SQLite em memória, permitindo executar verificações isoladas sem depender do banco SQL Server de desenvolvimento.

## Próximas etapas

- [x] Estrutura inicial da solução.
- [x] Separação das camadas da Clean Architecture.
- [x] Implementação das entidades iniciais.
- [x] Configuração do Entity Framework Core.
- [x] Primeira migration.
- [x] Implementação do EmployeeRepository.
- [x] Testes automatizados iniciais.
- [x] Configuração do Swagger.
- [ ] Implementação dos demais repositórios.
- [ ] Desenvolvimento dos casos de uso.
- [ ] Implementação da autenticação e verificação de colaboradores.
- [ ] Integração com WhatsApp.
- [ ] Integração com Inteligência Artificial.
- [ ] Implementação da base de conhecimento.
- [ ] Sistema de notificações e agenda.

---

**CentralITAssistant** — Projeto desenvolvido em C# e .NET, com foco em arquitetura limpa, automação e Inteligência Artificial aplicada ao ambiente corporativo.
