# 🌐 .NET Microservices Architecture  

Welcome to the **.NET Microservices Application** repository! This project is an enterprise-grade solution designed to leverage the power of microservices architecture, ensuring scalability, flexibility, and maintainability. Each service is built with **clean architecture** principles combined with a **hexagonal design**, following industry best practices.  

## 🛠️ Key Features  

1. **Service Discovery with Consul**:  
   - All services register dynamically with **Consul**, enabling the API Gateway to discover and connect with them seamlessly.  
   
   <img src="screenshots/consul-services.png" alt="Consul Active Services" height="600"/>

2. **Inter-service Communication with RabbitMQ**:  
   - Reliable and efficient communication between microservices is handled through **RabbitMQ**, ensuring event-driven architecture and asynchronous processing.  

3. **Distributed Tracing and Monitoring**:  
   - Integrated **OpenTelemetry** for observability across the system.  
   - Health checks ensure each service is operating optimally.  

4. **JWT Authentication**:  
   - Secured endpoints using **JWT tokens**, enhancing authentication and authorization mechanisms.  

5. **Microservices**:  
   - **Auth Service**: Handles user authentication and authorization.  
   - **Doctors Service**: Manages doctor information and schedules.  
   - **Patients Service**: Manages patient records and interactions.  
   - **Diagnoses Service**: Stores and retrieves medical diagnoses.  

6. **Databases**:  
   - **PostgreSQL**: Relational database for structured data.  
   - **MongoDB**: NoSQL database for document-oriented storage.  
   - **Redis**: In-memory database for caching and session storage.  

7. **Design Patterns**:  
   - **CQRS**: Separates write and read operations for scalability.  
   - **Request/Response**: Ensures clear communication between components.  
   - **Decorator**: Enhances functionality without modifying core logic.  
   - **Repository, Factory, Builder, Singleton**: Applied for maintainability and reusability.  

8. **Modern Tools and Libraries**:  
   - **EF Core** with **Fluent API** for model definitions and migrations.  
   - **MassTransit** for RabbitMQ integration.  
   - **Fluent Validations** for robust input validation.  
   - **MongoDB.Driver** for interacting with MongoDB.  

9. **API Gateway**:  
   - Middleware handles query validation, routing, and enhances security.  

10. **Best Practices**:  
   - Adheres to **SOLID**, **KISS**, and object-oriented principles.  
   - Utilizes **compiled queries** in EF Core for performance optimization.  
   - Built with testability and maintainability in mind.  

11. **Full-Text Search with Elasticsearch**:  
   - Integrated **Elasticsearch** for advanced search capabilities across services.  
   - Allows for powerful and fast full-text search on structured and unstructured data.  
   - Supports advanced querying and filtering, making it ideal for services such as **Doctors** and **Diagnoses** to quickly retrieve records based on multiple search criteria.
   - Elasticsearch provides scalability and flexibility in querying large datasets, ensuring low-latency responses.

12. **Caching and Decorator Pattern**:  
   - **Decorator Pattern** applied to repository layer to enhance caching functionality.  
   - Utilizes **Redis** and **IDistributedCache** to cache frequently accessed data, improving performance and reducing database load.  
   - The decorator ensures that repositories can store and retrieve data from the cache seamlessly, falling back to the database if data is not found in the cache.  
   - This pattern promotes clean separation of concerns and allows easy extension of caching functionality across services.

## 🏗️ Architecture  

- **Microservices**: Each service is independently deployable and encapsulates its own domain logic.  
- **Clean Architecture**: Ensures separation of concerns with clearly defined layers.  
- **Hexagonal Design**: Promotes testability and isolates the core logic from external dependencies.  

## 🚀 How It Works  

1. **Service Registration**:  
   - Each service registers with **Consul**, making itself discoverable to the API Gateway.  

2. **Communication**:  
   - Asynchronous messaging via **RabbitMQ** for decoupled and reliable inter-service communication.  

3. **Observability**:  
   - Health checks and distributed tracing through **OpenTelemetry**, ensuring smooth debugging and performance monitoring.  

4. **Secure Access**:  
   - All API requests are validated and authenticated with **JWT**, ensuring secure communication.  

5. **Full-Text Search**:  
   - Elasticsearch indexes are used to provide fast and accurate full-text search capabilities across various services.
   - Allows the system to handle complex queries on large datasets, enhancing search functionality within **Doctors** and **Diagnoses** services.  
   - Elasticsearch integrates seamlessly with the backend, making it easy to query for medical records, doctor schedules, and other relevant data efficiently.  

6. **Caching with Redis and Decorator Pattern**:  
   - Repositories are enhanced with caching using **Redis** via the **IDistributedCache** interface.  
   - The **Decorator Pattern** ensures that each repository can cache its results, improving performance by reducing the need for frequent database queries.  
   - If the requested data is not in the cache, the repository fetches it from the database and stores it in Redis for future use.

## 🤝 Why Choose This Architecture?  

This project is designed for enterprise-grade applications, where scalability, reliability, and maintainability are critical. By combining microservices architecture with clean and hexagonal design principles, it ensures:  
- High performance and low coupling.  
- Seamless scaling of individual services.  
- Future-proof architecture for evolving business needs.  
- Enhanced search capabilities with **Elasticsearch**, improving usability and performance.  
- Improved response times and reduced database load using **Redis** caching with the **Decorator Pattern**.

---

Feel free to explore the repository and let me know if you’re interested in collaborating or learning more about the system. Together, we can build innovative and reliable solutions!
