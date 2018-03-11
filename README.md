# Bygone
Yet another event store library

## What?
A simple bare bones event store library that does not try to reinvent the wheel. Be as simple and do just enough to store streams of events and get them back.

Comes with simple serialization based on https://github.com/mgravell/protobuf-net, but you can implement your own.

## Where to get it?
Here, but should probably do some nuget packages

### Persistence
At the moment comes with
* Microsoft SQL Server persistence
* Simple in memory persistence

You can implement your own and check if it works as expected using the base classes in `Bygone.PersistenceTests`

#### Microsoft SQL Server
As I want this to be as simple as possible, there is no need to reinvent a whole storage backend. Use SQL Express or a High Available SQL Azure setup and you're good to go.

#### In memory
Probably not that great, but is probably ok when using it as part of some unit testing..

## Integration tests
The persistence implementations can be tested in isolation using Docker and the docker-compose file in the root of the project.

I'm a fan of Docker

## What is missing?
Not much, it's meant to be simple. Maybe some more tests

## Ideas
* Maybe some ASP.NET Core middleware stuff to expose the streams
* Projections so views and other stuff can be built from the streams



