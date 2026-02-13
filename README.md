# 🛒 ECommerce Microservices Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat&logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?style=flat&logo=redis)](https://redis.io/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-FF6600?style=flat&logo=rabbitmq)](https://www.rabbitmq.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> A production-ready microservices-based e-commerce platform built with .NET 8, demonstrating modern cloud-native patterns and enterprise-grade architecture.

![Microservices Architecture](https://via.placeholder.com/800x400.png?text=Microservices+Architecture+Diagram)

## Architecture Overview

This solution implements a microservices architecture with the following services:

|     Service      |          Technology            |  Database  |               Description                   |
|----------------- |--------------------------------|------------|---------------------------------------------|
| **Catalog API**  | .NET 8, Carter, MediatR, Marten| PostgreSQL | Product management with CQRS                |
| **Basket API**   | .NET 8, Redis, MediatR         | Redis      | Shopping cart with distributed caching      |
| **Ordering API** | .NET 8, EF Core, MediatR       | PostgreSQL | Order processing with DDD                   |
| **API Gateway**  | YARP (Reverse Proxy)           |      -     | Single entry point, request routing         |
| **Event Bus**    | RabbitMQ, Polly                |      -     | Async messaging, event-driven communication |

## Key Features

- Clean Architecture - Separation of concerns with Domain, Application, Infrastructure layers
- CQRS Pattern - Command Query Responsibility Segregation with MediatR
- Event-Driven Architecture - Async communication via RabbitMQ
- Repository Pattern - Abstracted data access layer
- API Gateway Pattern - Centralized request routing with YARP
- Distributed Caching - Redis for basket storage
- Resilience - Polly retry policies for RabbitMQ connections
- Structured Logging - Serilog with Seq integration
- Minimal APIs - Modern Carter library for lightweight endpoints

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (optional)

### Installation

```bash
# Clone the repository
git clone https://github.com/Gohitha02/ECommerceMicroservices.git
cd ECommerceMicroservices

# Restore packages
dotnet restore

# Build solution
dotnet build

Running the Services:
Each service runs on a different port:
| Service      | URL                     | Command                                                   |
| ------------ | ----------------------- | --------------------------------------------------------- |
| API Gateway  | <http://localhost:5000> | `dotnet run --project src/APIGateway`                     |
| Catalog API  | <http://localhost:5001> | `dotnet run --project src/Services/Catalog/Catalog.API`   |
| Basket API   | <http://localhost:5002> | `dotnet run --project src/Services/Basket/Basket.API`     |
| Ordering API | <http://localhost:5003> | `dotnet run --project src/Services/Ordering/Ordering.API` |

API Endpoints:
Catalog Service
GET    /api/products          # List all products (paginated)
GET    /api/products/{id}     # Get product by ID
POST   /api/products          # Create new product
PUT    /api/products/{id}     # Update product
DELETE /api/products/{id}     # Delete product

Basket Service
GET    /api/basket/{username} # Get user's shopping cart
POST   /api/basket            # Store/update basket
DELETE /api/basket/{username} # Delete basket

Ordering Service
GET    /api/orders            # List all orders (paginated)
POST   /api/orders            # Create new order

--->Tech Stack
=>Backend
.NET 8 - Primary framework
Carter - Minimal API routing
MediatR - CQRS and mediator pattern
Marten - PostgreSQL as Document DB
Entity Framework Core - ORM for relational data
FluentValidation - Input validation
Mapster - Object mapping
Serilog - Structured logging

=>Infrastructure
PostgreSQL - Primary database
Redis - Distributed caching
RabbitMQ - Message broker
YARP - Reverse proxy / API Gateway
Polly - Resilience and retry policies

Project Structure:
ECommerceMicroservices/
├── src/
│   ├── APIGateway/                    # YARP Reverse Proxy
│   ├── BuildingBlocks/
│   │   └── EventBus/                  # Shared messaging library
│   │       ├── Abstractions/
│   │       ├── Events/
│   │       └── Implementations/
│   └── Services/
│       ├── Catalog/                   # Product catalog service
│       │   └── Catalog.API/
│       ├── Basket/                    # Shopping cart service
│       │   └── Basket.API/
│       └── Ordering/                  # Order management service
│           └── Ordering.API/
└── README.md

Design Patterns Implemented:
| Pattern                  | Implementation                    |
| ------------------------ | --------------------------------- |
| **Clean Architecture**   | Clear separation between layers   |
| **CQRS**                 | Commands and Queries with MediatR |
| **Event-Driven**         | RabbitMQ for async communication  |
| **Repository**           | Abstracted data access            |
| **Dependency Injection** | Built-in .NET DI container        |
| **API Gateway**          | Single entry point for clients    |

Future Enhancements
[ ] Add Identity Service with JWT authentication
[ ] Implement Payment Service with Stripe integration
[ ] Add containerization with Docker
[ ] Implement health checks and monitoring
[ ] Add unit and integration tests
[ ] Deploy to Azure Kubernetes Service (AKS)

About Me
I'm a .NET Developer with 5+ years of experience building scalable, enterprise-grade applications. This project demonstrates my expertise in:
- Microservices architecture
- Distributed systems design
- Cloud-native development
- Modern .NET ecosystem
Connect with me:
LinkedIn: www.linkedin.com/in/gohitha-r
Email: gohitha02@gmail.com

License
This project is licensed under the MIT License - see the LICENSE file for details.
