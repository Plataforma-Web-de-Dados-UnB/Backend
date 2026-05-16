# Portal de Dados UnB - API

API REST para o Portal de Dados da Universidade de Brasília.

## Tecnologias

- .NET 10.0
- ASP.NET Core Identity
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger/OpenAPI

## Configuração

### Pré-requisitos

- .NET 10 SDK
- PostgreSQL 12+

### Banco de Dados

1. Crie um banco de dados PostgreSQL:

```sql
CREATE DATABASE unb_portal_dev;
```

2. Configure a connection string em `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=unb_portal_dev;Username=postgres;Password=postgres"
  }
}
```

### Executar Migrations

```bash
dotnet ef database update
```

### Executar a Aplicação

```bash
dotnet run
```

A API estará disponível em `https://localhost:5001` ou `http://localhost:5000`.

## Usuário Padrão

Um Super Administrador é criado automaticamente na primeira execução:

- **Email**: admin@unb.br
- **Senha**: Admin123!
- **Cargo**: SuperAdministrador
- **Status**: Ativo

## Endpoints

### Autenticação (Público)

- `POST /Usuario/register` - Cadastro de administrador (status Pendente)
- `POST /Usuario/login` - Login

### Perfil (Autenticado)

- `GET /Usuario/perfil` - Ver perfil próprio
- `PUT /Usuario/senha` - Redefinir senha

### Administração (SuperAdmin)

- `GET /Admin/usuarios` - Listar administradores (com filtros)
- `GET /Admin/usuarios/{id}` - Detalhes de um administrador
- `PUT /Admin/usuarios/{id}/status` - Aprovar/rejeitar/revogar acesso

## Documentação da API

Acesse `/swagger` quando a aplicação estiver rodando em modo Development.

## Estrutura do Projeto

```
Backend/
├── Controllers/        # Endpoints da API
├── Models/            # Entidades do domínio
├── Views/             # DTOs
├── Services/          # Lógica de negócio
├── Data/              # Contexto do banco de dados
├── Helpers/           # Classes auxiliares
└── Migrations/        # Migrations do EF Core
```

## Perfis de Usuário

### Visitante
- Acesso apenas à área pública do portal
- Visualização de indicadores, painéis, categorias e glossário

### Administrador
- Todas as permissões de Visitante
- Gerenciamento de painéis, categorias, glossário e dados
- Manutenção operacional da plataforma
- **Requer aprovação do SuperAdmin**

### SuperAdministrador
- Todas as permissões de Administrador
- Gerenciamento de usuários administradores
- Controle de permissões (aprovar/rejeitar/revogar)

## Fluxo de Aprovação

1. Administrador se cadastra (status: Pendente)
2. SuperAdmin aprova ou rejeita o cadastro
3. Apenas usuários com status Ativo podem fazer login
4. SuperAdmin pode revogar acesso (Ativo -> Recusado)
