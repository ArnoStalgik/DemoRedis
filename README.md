# DemoRedis

**DemoRedis** est une application .NET 6+ de démonstration utilisant Redis comme cache distribué via l’interface standard `IDistributedCache`. Elle illustre l’utilisation d’un cache pour accélérer l’accès à des données simulées lentes.

## 🚀 Fonctionnalités

- Utilisation du cache Redis avec expiration (TTL).
- Exemple de pattern "cache aside" pour le contrôle des accès.
- API RESTful avec Swagger UI.
- Architecture claire : controllers, services, modèles.
- Facilement extensible.

## 🧑‍💻 Stack technique

- .NET 6+
- ASP.NET Core WebAPI
- StackExchange.Redis (via `IDistributedCache`)
- Swagger / OpenAPI

## 🔧 Prérequis

- [.NET 6+ SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) pour lancer Redis facilement :

```bash
docker run -d -p 6379:6379 redis
