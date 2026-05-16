# Exemplos de Requisições - API Portal UnB

## 1. Cadastro de Administrador

```bash
curl -X POST https://localhost:5001/Usuario/register \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João",
    "ultimoNome": "Silva",
    "email": "joao.silva@unb.br",
    "senha": "Senha123!"
  }'
```

**Resposta de Sucesso:**
```json
{
  "message": "Cadastro realizado com sucesso. Aguarde a aprovação do administrador."
}
```

## 2. Login

### 2.1 Login como SuperAdmin

```bash
curl -X POST https://localhost:5001/Usuario/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@unb.br",
    "senha": "Admin123!"
  }'
```

**Resposta de Sucesso:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "id": "abc123",
  "nome": "Super",
  "ultimoNome": "Administrador",
  "email": "admin@unb.br",
  "cargo": 0
}
```

### 2.2 Login com Usuário Pendente

```bash
curl -X POST https://localhost:5001/Usuario/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao.silva@unb.br",
    "senha": "Senha123!"
  }'
```

**Resposta de Erro:**
```json
{
  "message": "Seu cadastro está pendente de aprovação pelo administrador."
}
```

## 3. Ver Perfil Próprio

```bash
curl -X GET https://localhost:5001/Usuario/perfil \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

**Resposta de Sucesso:**
```json
{
  "id": "abc123",
  "nome": "Super",
  "ultimoNome": "Administrador",
  "email": "admin@unb.br",
  "cargo": 0,
  "status": 1,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

## 4. Redefinir Senha

```bash
curl -X PUT https://localhost:5001/Usuario/senha \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "senhaAntiga": "Admin123!",
    "senhaNova": "NovaSenha123!"
  }'
```

**Resposta de Sucesso:**
```json
{
  "message": "Senha alterada com sucesso."
}
```

## 5. Listar Administradores (SuperAdmin)

### 5.1 Listar Todos

```bash
curl -X GET https://localhost:5001/Admin/usuarios \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

### 5.2 Filtrar por Status Pendente

```bash
curl -X GET "https://localhost:5001/Admin/usuarios?status=0" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

### 5.3 Buscar por Nome/Email

```bash
curl -X GET "https://localhost:5001/Admin/usuarios?busca=joao" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

### 5.4 Combinar Filtros

```bash
curl -X GET "https://localhost:5001/Admin/usuarios?status=0&busca=silva" \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

**Resposta de Sucesso:**
```json
[
  {
    "id": "def456",
    "nome": "João",
    "ultimoNome": "Silva",
    "email": "joao.silva@unb.br",
    "cargo": 1,
    "status": 0,
    "createdAt": "2024-01-02T00:00:00Z"
  }
]
```

## 6. Ver Detalhes de Administrador (SuperAdmin)

```bash
curl -X GET https://localhost:5001/Admin/usuarios/def456 \
  -H "Authorization: Bearer SEU_TOKEN_JWT"
```

**Resposta de Sucesso:**
```json
{
  "id": "def456",
  "nome": "João",
  "ultimoNome": "Silva",
  "email": "joao.silva@unb.br",
  "cargo": 1,
  "status": 0,
  "createdAt": "2024-01-02T00:00:00Z",
  "updatedAt": "2024-01-02T00:00:00Z"
}
```

## 7. Aprovar Administrador (SuperAdmin)

```bash
curl -X PUT https://localhost:5001/Admin/usuarios/def456/status \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "status": 1
  }'
```

**Resposta de Sucesso:**
```json
{
  "message": "Usuário aprovado com sucesso."
}
```

## 8. Rejeitar Administrador (SuperAdmin)

```bash
curl -X PUT https://localhost:5001/Admin/usuarios/def456/status \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "status": 2
  }'
```

**Resposta de Sucesso:**
```json
{
  "message": "Usuário recusado com sucesso."
}
```

## 9. Revogar Acesso (SuperAdmin)

```bash
curl -X PUT https://localhost:5001/Admin/usuarios/def456/status \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "status": 2
  }'
```

**Resposta de Sucesso:**
```json
{
  "message": "Usuário recusado com sucesso."
}
```

## Códigos de Status

### CargoUsuario (Enum)
- `0`: SuperAdministrador
- `1`: Administrador
- `2`: Visitante

### StatusUsuario (Enum)
- `0`: Pendente
- `1`: Ativo
- `2`: Recusado

## Códigos HTTP

- `200 OK`: Requisição bem-sucedida
- `400 Bad Request`: Dados inválidos ou erro de validação
- `401 Unauthorized`: Token inválido ou ausente
- `403 Forbidden`: Sem permissão para acessar o recurso
- `404 Not Found`: Recurso não encontrado

## Observações

1. Substitua `SEU_TOKEN_JWT` pelo token recebido no login
2. O token JWT expira em 2 horas
3. Apenas SuperAdministradores podem acessar endpoints `/Admin/*`
4. Usuários com status Pendente ou Recusado não podem fazer login
5. Não é possível alterar o status de um SuperAdministrador
