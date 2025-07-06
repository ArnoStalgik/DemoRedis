# DemoRedis

DemoRedis est une API de démonstration .NET 8 illustrant l’usage professionnel de Redis comme cache distribué, la structuration clean, l’injection de dépendances par interface, le logging moderne et les tests unitaires robustes. Ce projet sert de modèle pour toute équipe cherchant à industrialiser ses APIs .NET !

---

## 🚀 Fonctionnalités

- **Cache distribué Redis** avec configuration centralisée
- **Injection de dépendances par interface** (IProductService)
- **Logging professionnel** (Serilog)
- **Gestion d’erreurs globale** (middleware custom)
- **Healthcheck Redis** intégré (`/health`)
- **Tests unitaires** (xUnit + Moq, coverage complet des controllers/services)
- **API REST documentée** (Swagger/OpenAPI)
- **Pipeline prêt pour CI/CD** (GitHub Actions, Azure DevOps, etc.)

---

## 🛠️ Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) pour Redis :
  ```bash
  docker run -d -p 6379:6379 redis
  ```

---

## ▶️ Lancer le projet

```bash
git clone https://github.com/arnaud-wissart/DemoRedis.git
cd DemoRedis
dotnet restore
dotnet run
```
- Accès Swagger : http://localhost:5000/swagger  
- Healthcheck : http://localhost:5000/health

---

## ⚙️ Configuration

`appsettings.json` :

```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "DemoRedisApi:"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": "Information",
    "WriteTo": [ { "Name": "Console" } ],
    "Enrich": [ "FromLogContext" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🏗️ Architecture

- **Controllers/**
  - `ProductController` : endpoints de gestion produits avec cache Redis (pattern cache-aside)
  - `CacheDemoController` : endpoints génériques pour manipuler le cache (get/set/delete)
- **Services/**
  - `IProductService` : interface métier
  - `SlowProductService` : implémentation simulant une “DB” lente, testable/mockable
- **Middlewares/**
  - `ErrorHandlingMiddleware` : capture & log des erreurs inattendues
- **Models/**
  - `Produit` : DTO principal

---

## 🧪 Tests unitaires

- **Frameworks** : xUnit, Moq
- **Couvre** :
  - Toutes les branches des controllers (cache hit/miss, erreurs, succès)
  - Les interactions avec Redis (via `IDistributedCache`)
  - La logique de fallback vers la “DB”
- **Exécution** :
  ```bash
  dotnet test
  ```

---

## 🩺 Healthcheck

- Endpoint `/health`  
- Vérifie la disponibilité de Redis en temps réel
- Idéal pour le monitoring Docker/K8S/Cloud

---

## 📝 Bonnes pratiques

- **Aucune chaîne en dur** : tout passe par config/env.
- **Tests isolés du backend** : pas besoin de serveur en prod pour les unit tests.
- **Interface partout pour les dépendances** : facilite l’extension et le test.
- **Logging riche et contextualisé**.
- **Réponses API structurées et normalisées**.

---

## 💡 Exemples d’appels

- **GET** `/Product/123`  
  Récupère le produit 123 (cache ou “DB” si miss)
- **GET** `/CacheDemo/maCle`  
  Lit la clé brute “maCle” du cache
- **POST** `/CacheDemo/maCle`  
  Body = string, met en cache pour 5 minutes
- **DELETE** `/CacheDemo/maCle`  
  Supprime la clé

---

## 📦 CI/CD

- Tests et healthcheck compatibles avec tous les principaux pipelines (GitHub Actions, Azure DevOps…).
- Dockerfile possible (non fourni ici, mais trivial à faire).

---

## 🏅 Crédit & Licence

MIT © Arnaud Wissart

---

**Repo géré, testé, documenté et validé par Arnaud Wissart**  
[https://github.com/arnaud-wissart](https://github.com/arnaud-wissart)

---

> **Besoin d’un projet de référence .NET ? Tu es au bon endroit.**  
> Teste, fork, challenge, et améliore encore ce repo !
