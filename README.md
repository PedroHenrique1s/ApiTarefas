# 📋 API de Tarefas

API REST desenvolvida em ASP.NET Core que permite o gerenciamento de tarefas com autenticação via JWT, incluindo **Refresh Token** para sessões mais robustas. A aplicação possui rotas protegidas, autenticação segura e relacionamento entre usuários e suas tarefas.

---

## 🚀 Tecnologias Utilizadas

- **.NET 9 (Preview)**
- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT (JSON Web Token)
- Swagger/OpenAPI

---

## 🛠️ Funcionalidades

### 🔐 Autenticação
- `POST /login` - Realiza o login com e-mail e senha. Retorna um **token JWT (Access Token)** e um **Refresh Token**.
- `POST /refresh-token` - Permite a renovação do Access Token utilizando o Refresh Token.
- `GET /logout` - Revoga o token atual (via blacklist em memória), encerrando a sessão.

### 👤 Usuários
- `GET /usuarios` - Lista todos os usuários, incluindo suas tarefas associadas.
- `POST /usuarios` - Cria um novo usuário com nome, e-mail e senha.

### ✅ Tarefas (Acesso Autenticado)
- `GET /tarefas` - Lista as tarefas do usuário autenticado. Suporta **paginação**, **filtros por ID, status e busca textual**, e inclui informações do usuário associado.
- `GET /tarefas/{id}` - Retorna uma tarefa específica, incluindo detalhes do usuário que a criou.
- `POST /tarefas` - Cria uma nova tarefa vinculada ao usuário autenticado.
- `PUT /tarefas/{id}` - Atualiza os dados de uma tarefa existente.
- `DELETE /tarefas/{id}` - Remove uma tarefa.

---

## 🔐 Autenticação JWT

A autenticação é baseada em JWT e agora inclui um mecanismo de **Refresh Token** para melhorar a experiência do usuário e a segurança. Para acessar as rotas protegidas (tarefas), você deve seguir estes passos:

1.  **Fazer login** em `/login` com seu e-mail e senha.
2.  Você receberá um **Access Token** (o `token` principal) e um **Refresh Token**.
3.  Utilize o **Access Token** no Swagger (ou nos headers da sua requisição) para acessar as rotas protegidas.
4.  Quando o Access Token expirar, utilize o **Refresh Token** na rota `/refresh-token` para obter um novo par de tokens sem precisar fazer login novamente.

```json
{
  "token": "seu_access_token_aqui",
  "refreshToken": "seu_refresh_token_aqui",
  "user": {
    "id": 1,
    "email": "usuario@example.com"
  }
}
