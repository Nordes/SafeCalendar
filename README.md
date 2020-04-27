# SAFE Calendar

Using the [SAFE Stack](https://safe-stack.github.io/), I wanted to build something a bit different from the sample available on the website to experiment and learn a bit. The quality of the code is not something I am looking for, but more experimenting the technology and how it all interacts together

## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET Core SDK](https://www.microsoft.com/net/download)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you can also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Work with the application

Before you run the project **for the first time only** you should install its local tools with this command:

```bash
dotnet tool restore
```


To concurrently run the server and the client components in watch mode use the following command:

```bash
dotnet fake build -t run
```

## How it works

The calendar have the following features

Client:
  * Add events
  * Edit events
  * Move accross months

Server: (All in TODO)
  * API's (OData or simple RESTful?)
    * Add/Update events (Not existing)
    * Get events for a particular set of dates
  * Database
    * Not existing so far... could be stored in text file or csv for now... even ICAL format... why not
