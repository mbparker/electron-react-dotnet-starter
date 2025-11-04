# electron-react-dotnet-starter

[![Build and Release .NET 9 API and Electron UI](https://github.com/mbparker/electron-react-dotnet-starter/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/mbparker/electron-react-dotnet-starter/actions)

Boilerplate project for cross platform Electron apps using React with MUI for the UI, and .NET 9 powering the backend.

UI is Typescript, and uses tsyringe for dependency injection. Webpack is used for building rather than Vite because it preserves the injected type metadata required by tsyringe. No framework (such as Next.Js) is used.

.NET backend is C# and uses Autofac for dependency injection. SQLite3 support is provided by LibSqlite3Orm.

SignalR is used for command and control communication between the front and backend. RESTful APIs are used for data access type requests.


# USAGE

1. Clone to your system (or just download the ZIP from Github).
2. Build `ElectronAppApi.sln` under the `app-api` directory.
3. Run the `ElectronAppRebrander.csproj` compiled output binary. You will be prompted to provide the root directory you cloned to, and what you want the new app name to be. Thats it.

The "rebranding" process will update the solution, project files, project directories and filenames, root namespace, Electron configuration files, package.json and other files to reflect your new name.

You can then begin building your own app using this repo as a launch pad.

# KNOWN ISSUES

* The LibSqlite3Orm project is has been improved, but is still slow when compared to some other ORMs. It was designed to be simple and easy to use for local app storage use cases. If you need a high performance ORM, you will need to swap it out for something like Dapper, or a full blown ORM such as Entity Framework Core. That said, performance is being worked on.
