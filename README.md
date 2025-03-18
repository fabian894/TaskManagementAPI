# TaskManagementAPI
Built a Microservices-Based Task Management API

Design and implement a backend RESTful API to manage tasks (e.g., create, retrieve, update, 
delete) using the specified technologies, with comprehensive API documentation provided via Swagger 
or Postman.

**Setup instructions (including database schema, dependencies, and how to run locally)**
Open project on VIsual Studio or Visual Studio Code
Open terminal or package console
run the following commands:
dotnet restore
dotnet clean
dotnet build
dotnet run - it will generate the running site http://localhost:5151
open postman with the 
dotnet test (unit & integration testing)
For cache: ensure Redis is installed on your local, then open command prompt and run **redis-cli**

If running on Visual Studio IDE
there is already a build and clean option on the upper side menu
then execute the project by clicking on the IIS Express with the green play icon and the swagger UI will open
then you can test the endpoints on the swagger UI according to the swaggerJSON API documentation being provided

**CQRS Implementation**
We implemented the Command Query Responsibility Segregation (CQRS) pattern to separate the write and read operations in our application.

**Why CQRS?**
Better Maintainability: Commands (Create, Update, Delete) and Queries (Read) are handled separately, making the code more organized and easier to modify.
Scalability: Commands and Queries can be optimized independently for performance.
Separation of Concerns: Prevents a single service from handling both reads and writes, reducing complexity.
How It Was Implemented
Commands (for modifying data)
CreateTaskCommandHandler
UpdateTaskCommandHandler
DeleteTaskCommandHandler
Queries (for retrieving data)
GetAllTasksQueryHandler
GetTaskByIdQueryHandler
Each command/query was handled using MediatR, ensuring a clean, decoupled architecture.

**Design Patterns Used**
To ensure a scalable, maintainable, and testable architecture, i incorporated the following design patterns:

i. Mediator Pattern (via MediatR)
Reduces direct dependencies between handlers and controllers.
Helps in making the code modular and scalable.
ii. Repository Pattern (Simulated)
While i didn’t implement a separate repository, i kept data access within ApplicationDbContext.
If needed, this can be extended into a full repository layer for better abstraction.
iii. Factory Pattern (for Creating Tasks)
The CreateTaskCommandHandler was designed to instantiate and return a new TaskEntity.
This can later be extended to use a factory for better object creation logic.

**State Management**
The API follows a stateless approach, meaning:

The API only processes requests and returns responses.
Tasks are stored persistently in the database (handled by ApplicationDbContext using Entity Framework Core).
State changes (task creation, updates, and deletions) are handled through command handlers, most especially for the update as i implemented a finite state machine for task status transitions i.e., Pending → In 
Progress → Completed.

**Database and Persistence**
I used Entity Framework Core (EF Core) with a relational database (SQL Server or SQLite for testing) as the ORM to manage data persistence.

ApplicationDbContext serves as the main data access layer, directly interacting with the database.
In-memory database was used in tests to simulate real-world behavior.

**Testing Strategy**
I implemented unit and integration tests using xUnit, Moq, FluentAssertions, and EF Core In-Memory:

Unit Tests ensure that each command/query handler behaves as expected.
Integration Tests ensures API endpoints work correctly.

Implementing Redis Caching for Query Optimization
To improve query performance, i integrated Redis to cache frequently accessed data, reducing database load.

Why Use Redis for Caching?
Faster Read Operations: Queries return data instantly without hitting the database.
Reduced Database Load: Minimizes redundant queries.
Optimized Performance: Ideal for GetAllTasks and GetTaskById.
