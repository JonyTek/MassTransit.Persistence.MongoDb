# MassTransit.Persistence.MongoDb
This package allows sagas to be stored in MongoDB. This means that you can orchestrate a long-running process and manage its state throughout execution.

[![Build status](https://ci.appveyor.com/api/projects/status/wqq4lp9d4b5x9b76?svg=true)](https://ci.appveyor.com/project/Liberis/masstransit-persistence-mongodb/branch/master)

## Getting Started
MassTransit.Persistence.MongoDb can be installed via the package manager console by executing the following commandlet:

```powershell
PM> Install-Package MassTransit.MongoDbIntegration
```

Once we have the package installed, we can create a `MongoDbSagaRepository` using one of the following constructors:

```csharp
var repository = new MongoDbSagaRepository(new MongoUrl("mongodb://localhost/masstransitTest"));
```

Or

```csharp
var repository = new MongoDbSagaRepository("mongodb://localhost", "masstransitTest"));
```

## Initiating a Simple Saga
Say we have an `InitiateSimpleSaga` message:

```csharp
class InitiateSimpleSaga :
        CorrelatedBy<Guid>
    {
        public InitiateSimpleSaga(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
    }
```

And we have a `SimpleSaga` that is initiated by the `InitiateSimpleSaga` message:

```csharp
class SimpleSaga :
        InitiatedBy<InitiateSimpleSaga>,
        IVersionedSaga
    {
        public Guid CorrelationId { get; set; }

        public int Version { get; set; }

        public Task Consume(ConsumeContext<InitiateSimpleSaga> context)
        {
            //Do some cool stuff...
            Console.WriteLine($"{nameof(InitiateSimpleSaga)} consumed");

            return Task.FromResult(0);
        }
    }

```

Now we have our saga and our initiation message, we can set up our bus to use the `MongoDbSagaRepository`:

```csharp
var busControl = Bus.Factory.CreateUsingInMemory(configurator =>
    {
        configurator.ReceiveEndpoint("my_awesome_endpoint", endpoint =>
        {
            //Normal receive endpoint config...

            endpoint.Saga(new MongoDbSagaRepository<SimpleSaga>(new MongoUrl("mongodb://localhost/masstransitTest")));
        });
    });
```

With everything now configured we can raise the `InitiateSimpleSaga` message to get the saga kicked off:

```csharp
var id = Guid.NewGuid();

var message = new InitiateSimpleSaga(id);

await busControl.Publish(message);
```

# Contribute
1. Fork
2. Hack!
3. Pull Request