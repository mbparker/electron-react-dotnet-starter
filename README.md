# electron-react-dotnet
Boilerplate project for cross platform Electron apps using React with MUI for the UI, and .NET 9 powering the logic.

UI is Typescript, and uses tsyringe for dependency injection. Webpack is used for building rather than Vite because it preserves the injected type metadata required by tsyringe. No framework (such as Next.Js) is used.

.NET backend is C# and uses Autofac for dependency injection. SQLite3 support is built in using a lightweight "ORM" mechanism.

SignalR is used for communication between the front and backend. RESTful APIs could be easily added if that is preferred.
