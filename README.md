# 📋 API de Tarefas

API REST desenvolvida em ASP.NET Core que permite o gerenciamento de tarefas com autenticação via JWT. A aplicação possui rotas protegidas, autenticação segura, e relacionamento entre usuários e suas tarefas.

---

## 🚀 Tecnologias Utilizadas

- [.NET 9 (Preview)](https://dotnet.microsoft.com/)
- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT (JSON Web Token)
- Swagger/OpenAPI

---

## 🛠️ Funcionalidades

### 🔐 Autenticação
- `POST /login` - Login com e-mail e senha. Retorna um token JWT.
- `GET /logout` - Revoga o token atual (via blacklist in-memory).

### 👤 Usuários
- `GET /usuarios` - Lista todos os usuários.
- `POST /usuarios` - Cria um novo usuário com nome, e-mail e senha.

### ✅ Tarefas (Acesso Autenticado)
- `GET /tarefas` - Lista todas as tarefas.
- `GET /tarefas/{id}` - Retorna uma tarefa específica.
- `POST /tarefas` - Cria uma nova tarefa vinculada a um usuário.
- `PUT /tarefas/{id}` - Atualiza os dados da tarefa.
- `DELETE /tarefas/{id}` - Remove uma tarefa.

---

## 🔐 Autenticação JWT

A autenticação é baseada em JWT. Para acessar as rotas protegidas (tarefas), você deve:

1. Fazer login em `/login`.
2. Copiar o token retornado.
3. Inserir o token no Swagger (ou nos headers da sua requisição):
