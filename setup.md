# Setup del Entorno

## Tecnologías

| Componente | Tecnología | Versión |
|------------|------------|----------|
| Frontend | Angular | 21 |
| Runtime Front | Node.js | 22 |
| Backend | ASP.NET Core | 9 |
| Base de Datos | PostgreSQL | 17 |
| Contenedores | Docker Desktop (Docker + Docker Compose) | Última |
| API Testing | Postman (opcional) | Última |

---

## Instalación

### Docker

Seguir los pasos de: `https://docs.docker.com/desktop/setup/install/linux/ubuntu`

### .NET SDK

Seguir los pasos de `.NET9`: `https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install`

### Node.js

Instalar NVM:

```bash
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/master/install.sh | bash
source ~/.bashrc
```

Instalar Node:

```bash
nvm install 22
nvm use 22
```

### Angular CLI

```bash
npm install -g @angular/cli
```

### Entity Framework
```bash
dotnet tool install --global dotnet-ef
```

---

## Verificación rápida

```bash
docker --version
docker compose version
dotnet --version
node -v
npm -v
ng version
```

---

# Inicialización

## Variables de entorno
- Crear un archivo `.env` en la raiz del proyecto y completar sus valores utilizando `.env.example` de referencia.

## Frontend

- Crear proyecto: `ng new frontend`
- Levantar proyecto: `ng s -o`

## Backend

- Crear proyecto: `dotnet new webapi -n backend`
- Levantar proyecto: `dotnet run`
- Crear migración: `dotnet ef migrations add nombre_migracion`

## BBDD
- Definir el `docker-compose.yml`
- Levantar DB: `docker compose up -d postgres`
- Verificar estado: `docker ps`
- Entrar a la terminal del contenedor: `docker exec -it algo15-postgres bash`
- Entrar a la DB: `docker exec -it algo15-postgres psql -U algo15_user -d algo15` 
- Apagar contenedor: `docker compose down`
- Eliminar volumen: `docker compose down -v`

**Importante: para aplicar las migrations y tener la DB actualizada aplicar `dotnet ef database update` después de hacer un `git pull`**

---

# Recursos útiles

## Frontend

- https://ui.angular-material.dev/blocks

## Extensiones recomendadas VS Code:

- C# Dev Kit
- Angular Language Service
- Docker
- GitLens
- ESLint
- Prettier
- PostgreSQL (de Microsoft): muy buena para hacer queries a la DB