# 💰 ExpenseManager — API de Gestión de Gastos

Sistema backend desarrollado en **.NET 10** con arquitectura hexagonal para gestionar gastos personales por categorías, con autenticación JWT y control de acceso por roles (Admin / User).

---

## 🏗️ Arquitectura del proyecto

```
ExpenseManager/
├── Api/                        → Controladores, AutoMapper, JWT, Program.cs
├── Application/                → Interfaces y DTOs
├── Domain/                     → Entidades de dominio
├── Infrastructure/             → DbContext, Repositorios, UnitOfWork
├── Frontend-gastos-react/      → Frontend React (entregado por el trainer)
│   └── frontend-gastos-react/
└── ExpenseManager.slnx
```

---

## ⚙️ Requisitos previos

Antes de ejecutar el proyecto asegúrate de tener instalado:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [MySQL 8+](https://dev.mysql.com/downloads/)
- [Node.js LTS](https://nodejs.org)
- dotnet-ef (herramienta de migraciones):

```bash
dotnet tool install --global dotnet-ef
```

---

## 🚀 Paso a paso para ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd ExpenseManager
```

---

### 2. Configurar las variables de entorno del backend

Navega a la carpeta `Api` y crea el archivo `.env`:

```bash
cd Api
```

Crea el archivo `.env` con este contenido (cambia la contraseña de MySQL por la tuya):

```env
MYSQL_CONNECTION=server=localhost;port=3306;database=expense_manager;user=root;password=1234;
JWT_KEY=CambiaMePorUnaClaveSecretaMuyLarga12345678
JWT_ISSUER=ExpenseManagerApi
JWT_AUDIENCE=ExpenseManagerClient
JWT_DURATION_MINUTES=120
FRONTEND_URL=http://localhost:5173
```

---

### 3. Crear la base de datos con migraciones

Vuelve a la raíz del proyecto y ejecuta:

```bash
cd ..
dotnet ef database update -p Infrastructure -s Api
```

Esto crea automáticamente la base de datos `expense_manager` en MySQL con todas las tablas.

---

### 4. Ejecutar el backend

```bash
cd Api
dotnet run
```

El backend queda corriendo en: **http://localhost:5196**

Al arrancar, el seeder crea automáticamente:
- Roles: `Admin` y `User`
- Categorías: `Alimentación`, `Transporte`, `Salud`, `Servicios`
- Usuario administrador de prueba

---

### 5. Configurar y ejecutar el frontend

Abre una **segunda terminal** (sin cerrar la del backend) y navega a la carpeta del frontend:

```bash
cd Frontend-gastos-react\frontend-gastos-react
```

Crea el archivo `.env` dentro de esa carpeta:

```env
VITE_API_URL=http://localhost:5196/api
```

Instala las dependencias:

```bash
npm install
```

Ejecuta el frontend:

```bash
npm run dev
```

El frontend queda corriendo en: **http://localhost:5173**

---

## 🌐 Puertos utilizados

| Servicio | URL |
|---|---|
| Backend (.NET 10) | `http://localhost:5196` |
| Frontend (React + Vite) | `http://localhost:5173` |
| MySQL | `localhost:3306` |

---

## 🔐 Variables de entorno requeridas

| Variable | Descripción |
|---|---|
| `MYSQL_CONNECTION` | Cadena de conexión a MySQL |
| `JWT_KEY` | Clave secreta para firmar el token (mínimo 32 caracteres) |
| `JWT_ISSUER` | Emisor del token |
| `JWT_AUDIENCE` | Audiencia del token |
| `JWT_DURATION_MINUTES` | Duración del token en minutos |
| `FRONTEND_URL` | URL del frontend React |

---

## 👤 Credenciales de prueba

### Administrador
| Campo | Valor |
|---|---|
| Email | `admin@gastos.com` |
| Contraseña | `Admin123*` |
| Rol | `Admin` |

### Usuario de prueba
| Campo | Valor |
|---|---|
| Email | `usuario@correo.com` |
| Contraseña | `Usuario123*` |
| Rol | `User` |

> El Admin ve todos los gastos y gestiona categorías. El User solo ve y opera sus propios gastos.

---

## 📋 Endpoints disponibles

### Autenticación (`/api/auth`)
| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/register` | Registrar usuario nuevo |
| POST | `/api/auth/login` | Iniciar sesión (genera cookie JWT) |
| POST | `/api/auth/logout` | Cerrar sesión (elimina cookie) |
| GET | `/api/auth/me` | Datos del usuario autenticado |

### Categorías (`/api/categories`)
| Método | Ruta | Acceso |
|---|---|---|
| GET | `/api/categories` | Admin / User |
| GET | `/api/categories/{id}` | Admin / User |
| POST | `/api/categories` | Solo Admin |
| PUT | `/api/categories/{id}` | Solo Admin |
| DELETE | `/api/categories/{id}` | Solo Admin |

### Gastos (`/api/expenses`)
| Método | Ruta | Acceso |
|---|---|---|
| GET | `/api/expenses` | Admin ve todos / User solo los suyos |
| GET | `/api/expenses?categoryId=1` | Filtra por categoría |
| GET | `/api/expenses/{id}` | Admin / User (403 si es ajeno) |
| POST | `/api/expenses` | Admin / User |
| PUT | `/api/expenses/{id}` | Admin / User (403 si es ajeno) |
| DELETE | `/api/expenses/{id}` | Admin / User (403 si es ajeno) |

---

## 🗄️ Modelo de datos

| Tabla | Descripción |
|---|---|
| `roles` | Roles del sistema (Admin, User) |
| `users` | Usuarios registrados |
| `user_roles` | Relación N:M entre usuarios y roles |
| `categories` | Categorías de gastos |
| `expenses` | Gastos de los usuarios |

---

## 📦 Tecnologías utilizadas

| Tecnología | Versión |
|---|---|
| .NET | 10 |
| Entity Framework Core | 10.0.1 |
| MySQL.EntityFrameworkCore | 10.0.1 |
| AutoMapper | 12.0.1 |
| JWT Bearer | 10.0.0 |
| BCrypt.Net-Next | 4.0.3 |
| DotNetEnv | 3.1.1 |
| React + Vite | - |

---

