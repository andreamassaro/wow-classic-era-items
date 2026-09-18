# AGENTS.md

## Structure
- Program.cs only wires things up and call to `app.Map<Entity>Endpoints()`
- Middleware lives in `Middleware`
- Program.cs owns application-wide, cross-cutting setup: anything that configures the host or the HTTP pipeline as a whole (e.g. OpenAPI/Swagger, logging/telemetry, authentication, CORS, exception handling, health checks). Register these directly in Program.cs.
- Registrations that belong to a specific project or feature (DbContext, clients, repositories, feature services) go in that project under `Config`. extension method called `Add<Layer>`. It's called from the preceeding layer. The order is api -> services -> repository. All the injections should be in a class called `<Project>Ìoc`.
- Rule of thumb: if removing a feature would make the registration unnecessary, it belongs with that feature; if it applies regardless of which features exist, it belongs in Program.cs.
- Endpoints under `Endpoints`, grouped by entity
- Each entity exposes a static `Map<Entity>Endpoints(this IEndpointRouteBuilder)` extension
- Request/response records sit next to their endpoint
- Business logic in csproj `<Solution>.Services`, not in endpoint lambdas
- `<Solution>.Repository` (class library) owns all external I/O: DbContext and migrations, repositories, HTTP clients, Service Bus/SFTP/storage clients, and their configuration/DI registration.
- `<Solution>Domain`(class library) owns all models, requests and static helper extensions.
- The api project references Services, never the reverse. 
- The Services project references Repository, never the reverse. 
- Endpoints and services depend on interfaces defined in Repository, not on DbContext or SDK clients directly.
- No HTTP concerns (request/response models, status codes) in Repository.
- Tests mirror the feature structure in `tests/<Project>.Tests/`
- Depend on concrete classes. Only introduce an interface when there are multiple implementations or a real need to substitute one (e.g. an external system that can't run in tests). No one-to-one interface/class pairs.
