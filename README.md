# RestMovies

RestMovies is a RESTful API project for managing movies, including their genres, ratings, and related operations. The repository follows a modular, layered architecture, primarily developed in C#.

## Main Folders

- **Movies.Api**  
  The entry point of the REST API. This project contains controllers, configuration, middleware, and API endpoint definitions for interacting with the movie management system.

- **Movies.Application**  
  Contains the core business logic, application services, and repositories for the movie domain. This includes models, data access logic, and business rules.

- **Movies.Contract**  
  Defines shared contracts such as data transfer objects (DTOs), API request/response models, and interfaces that facilitate communication between different layers or services.

- **Movies.Sdk**  
  Provides a Software Development Kit (SDK) to help external applications or consumers interact with the RestMovies API. It typically includes client classes and helpers.

- **Movies.SdkConsumer**  
  Demonstrates or implements an example client or consumer of the Movies SDK, showing how to integrate with the API using the provided SDK.

## Features

- Basic CRUD
- API versioning
- Swagger/OpenAPI documentation
- Authentication and authorization
- JWT based auth
- Api-key based auth
- Filtering, sorting and paginating
- HATEOAS
- Health checks
- Response and output caching
- Layered architecture with separation of concerns
- SDK for easy integration
- Example SDK consumer

## License

This project is licensed under the MIT License.
