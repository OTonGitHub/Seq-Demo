## Sources

- https://github.com/serilog/serilog/wiki/Getting-Started
- https://github.com/serilog/serilog/wiki/Configuration-Basics
- https://github.com/serilog/serilog/wiki/Enrichment
- https://github.com/serilog/serilog-settings-configuration
- https://github.com/serilog/serilog-aspnetcore#serilogaspnetcore---
- https://docs.datalust.co/docs/getting-started-with-docker
- https://www.youtube.com/watch?v=_iryZxv8Rxw&t=3059s
- https://www.pluralsight.com/courses/modern-structured-logging-serilog-seq

## Nuget Packages

- Serilog.AspNetCore v7.0.0
  - depends on `file & console` sinks
- PackageReference v7.0.1
- Serilog.Sinks.Seq v5.2.3

## .NET Commands

`dotnet new sln`

`dotnet new webapi`

`dotnet add reference from project to sln`

`install nuget packages`

`dotnet run --project API --launch-profile "https"`

## Setting Up

> in Program.cs

```
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true) // props: path, reloadOnChange
                                                   // reload if below configuration changes (,true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build()
    )
    .CreateLogger();

/* OT: USE ONLY FOR TESTING IF ANY ISSUES READING FROM APPSETTINGS, BELOW CONFIG IS
 * AN EXTENSION METHOD REPRESENTATION OF THE BASE STRUCTURE LAID  IN CONFIGURATION FILE, JUST
 * COPY THE CODE BELOW AND LEAVE COMMENTED AREA AS IS FOR TESTING.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        "logs/Serilog-FileSink.json",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [LC:{Level:u3}/UC:{Level:w3}] {Message:lj} {NewLine} {Properties:j}{NewLine}{Exception}"
    )
    .Enrich.With(new ThreadIDEnricher()) // order does-not matter, above and below sinks will have ThreadID property
        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
        .Enrich.WithProperty("Version", "6.9.6")
            .WriteTo.Seq("http://192.168.100.115:5341")
    .CreateLogger();
*/
```

<br/>

> And then Appsettings.json

<br />

```
 "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Seq"
    ],
    "MinimumLevel": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    },
    "LevelSwitches": {
      "$SeqLogLevel": "Verbose",
      "$ConsoleLogLevel": "Error"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "controlLevelSwitch": "$SeqLogLevel"
        }
      },
      { "Name": "File", "Args": { "path": "logs/Serilog-FileSink.json" } },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://192.168.1.113:5341",
          "apiKey": "",
          "controlLevelSwitch": "$SeqLogLevel"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
```

Read source code for anything that is not covered here.

## Notes

- have to setup flush and close all on app exit, back in .NET framework, application_end was in global asax,
  could possibly add to app shutdown hook, but not where to add in program.cs as of writing this.

- Specify production server specific settings in appsettings, and move sensitive stuff over to azure or other injection
  pipeline.

## Setting Up Docker

FOR ADDITIONIONAL INFORMATION: https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-20-04

`snap list`

`sudo snap remove docker`

`sudo apt update && apt upgrade`

> install a few prerequisite packages which let apt use packages over HTTPS:

`sudo apt install apt-transport-https ca-certificates curl software-properties-common -y`

> add the GPG key for the official Docker repository to your system:

`curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -`

> Add the Docker repository to APT sources

`sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable"`

> check if about to install from docker repo, not ubuntu repo

`apt-cache policy docker-ce`

- output from ^ should be like this:

  ````docker-ce:
  Installed: (none)
    Candidate: 5:24.0.6-1~ubuntu.20.04~focal
  Version table:
      5:24.0.6-1~ubuntu.20.04~focal 500
        2500 https://download.docker.com/linux/ubuntu focal/stable amd64 Packages```
  ````

`sudo apt install docker-ce`

> ADD Current User to Docker Group

`sudo usermod -aG docker ${USER}`

- ( -a ): safely add user to given group without removing from other group
- ( -G ): I think this just adds user to specified groups NOT SURE

> reset the shell for user (or just log out and log in)

`su - ${USER}`

`groups`

- CHECK INFO ON ALL USERS

  ```for username in $(cut -d: -f1 /etc/passwd); do
    echo "User: $username"
    groups "$username"
  id "$username"
  echo "-------------------"
  done
  ```

`sudo docker pull datalust/seq`

`docker images -a`

`docker volume create seq-vol`

`docker volume list`

> create and store hashed password in shell before starting container, or be reused in case restarts

`PH=$(echo 'Welcome@123' | sudo docker run --rm -i datalust/seq config hash)`

> if curious enough to see the hashed pass:

`echo $PH`

- **Finally:**

```sudo docker run \
--name seq \
-d \
--restart unless-stopped \
-e ACCEPT_EULA=Y \
-e SEQ_FIRSTRUN_ADMINUSERNAME="admin@csc" \
-e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$PH" \
-p 80:80 \
-p 5341:5341 \
-v seq-vol:/data \
datalust/seq
```
