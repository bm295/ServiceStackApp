# Wolverine playground

This .NET 10 ASP.NET Core sample lets you try [Wolverine](https://wolverinefx.net/introduction/getting-started) without a broker or database.

`POST /todos` calls `IMessageBus.InvokeAsync` to run the `CreateTodo` handler inline. The handler stores the todo and returns a `TodoCreated` event. Wolverine sends that cascading event to the local `todo-events` queue, where a second handler adds an activity entry.

## Project layout

| Path | Responsibility |
| --- | --- |
| `Program.cs` | Register storage, configure Wolverine routing, and map feature endpoints. |
| `Features/Todos/Create/` | One file each for the POST request, command, endpoint, and inline command handler. |
| `Features/Todos/Events/` | One file each for the `TodoCreated` event and its background handler. |
| `Features/Todos/Queries/` | Map read endpoints for todos and activity. |
| `Features/Todos/Models/` | Define the todo and activity data returned by the API. |
| `Storage/TodoStore.cs` | Keep todos and activity entries in memory. |

## Why use Wolverine?

The main benefit in this sample is that the API can complete the primary operation while the activity update runs as a separate message. Wolverine discovers the handlers by convention and routes the returned `TodoCreated` event to a local queue. See the [handler discovery](https://wolverinefx.net/guide/handlers/discovery) and [local queue](https://wolverinefx.net/guide/messaging/transports/local) documentation.

### Compared with a traditional implementation

| Concern | Direct service calls | Hand-built background queue | This Wolverine sample |
| --- | --- | --- | --- |
| Create a todo | The endpoint calls a service method. | The endpoint still calls a service method. | The endpoint invokes a `CreateTodo` command; Wolverine calls `CreateTodoHandler` inline. |
| Record activity | The endpoint or service calls the activity code directly and waits for it. | The endpoint writes to a `Channel`; a `BackgroundService` reads and dispatches it. | The command handler returns `TodoCreated`; Wolverine routes it to the `todo-events` queue and calls `TodoCreatedHandler`. |
| Wiring | Register and call services explicitly. | Build and register the channel, worker, dispatch logic, and error handling. | Register Wolverine and configure the event route; handlers are found by convention. |
| Best fit | A small workflow where all work should finish before responding. | A focused background task when a custom queue is acceptable. | A workflow with commands and events that may later need more messaging features. |

The difference in request flow is:

```text
Direct calls:  HTTP -> create service -> activity service -> response
Wolverine:     HTTP -> CreateTodoHandler -> response
                                |
                                +-> TodoCreated -> local queue -> TodoCreatedHandler
```

Wolverine can also be configured with [external transports](https://wolverinefx.net/guide/messaging/introduction) and [durable inbox/outbox messaging](https://wolverinefx.net/guide/durability/). Those features are **not enabled here**. The current local queue and `TodoStore` are in memory, so this example does not guarantee delivery across restarts. For a simple synchronous operation, a direct service call is usually easier to maintain.

## How it works

```text
HTTP client
    | POST /todos
    v
Minimal API (Program.cs)
    | IMessageBus.InvokeAsync(CreateTodo)
    v
CreateTodoHandler [runs inline]
    | saves Todo                         | returns TodoCreated
    v                                    v
TodoStore (memory)              local "todo-events" queue
    ^                                    |
    | records ActivityEntry              v
    +-------------------------- TodoCreatedHandler [background]

HTTP client -- GET /todos or /activity --> Minimal API --> TodoStore
```

The POST waits for `CreateTodoHandler` to finish. Processing `TodoCreated` on the local queue happens separately, so the activity feed can lag behind the created todo.

## Run

From the repository root:

```powershell
dotnet run --project src/WolverinePlayground --urls http://localhost:5080
```

In another terminal:

```powershell
$todo = Invoke-RestMethod http://localhost:5080/todos -Method Post -ContentType application/json -Body '{"title":"Try Wolverine"}'
Invoke-RestMethod "http://localhost:5080/todos/$($todo.id)"
Invoke-RestMethod http://localhost:5080/todos
Invoke-RestMethod http://localhost:5080/activity
```

The activity entry may appear shortly after the POST because the event runs on a local worker queue. Todos and activity are kept in memory and reset when the app stops. This example does not configure durable message delivery.
