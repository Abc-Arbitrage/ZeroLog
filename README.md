# ZeroLog

[![Build](https://github.com/Abc-Arbitrage/ZeroLog/workflows/Build/badge.svg)](https://github.com/Abc-Arbitrage/ZeroLog/actions?query=workflow%3ABuild)
[![NuGet](https://img.shields.io/nuget/v/ZeroLog.svg?label=NuGet&logo=NuGet)](http://www.nuget.org/packages/ZeroLog/)

ZeroLog is a **zero-allocation .NET logging library**. It uses the excellent formatting library [StringFormatter](https://github.com/MikePopoloski/StringFormatter).

  It provides basic logging capabilities to be used in latency-sensitive applications, where garbage collections are undesirable. ZeroLog can be used in a complete zero-allocation manner, meaning that after the initialization phase, it will not allocate any managed object on the heap, thus preventing any GC from being triggered.
  
  Since ZeroLog does not aim to replace any existing logging libraries in any kind of application, it won't try to compete on feature set level with more pre-eminent projets like log4net or NLog for example. The focus will remain on performance and allocation free aspects.
  
   The project is production ready and you can get it via Nuget if you want to give it a try. The package is available here:

>https://www.nuget.org/packages/ZeroLog/

## Internal design
 
 ZeroLog was designed to meet two main objectives:

  - Being a **zero allocation library**
  - Doing **as little work as possible in calling threads**

The second goal implies a major design choice: the actual logging is completely asynchronous. It means that writing messages to the appenders obviously occurs in a background thread, but also that *all formatting operations are delayed to be performed just before the appending*. **No formatting occurs in the calling thread**; the log data is merely marshalled to the background logging thread in the more efficient way possible.

 Internally, each logging call data (context, log messages, arguments, etc.) will be serialized to a pooled log event, before being enqueued in a concurrent data structure the background logging thread consumes. The logging thread will then format the log messages and append them to the configured appenders.

## Getting started

Before using ZeroLog, you need to initialize the `LogManager`. 
You have two options - either use a json configuration file, or use a programmatic setup:

```csharp
BasicConfigurator.Configure(new[] { new ConsoleAppender() });
```
As of today, the library comes with two existing appender implementations:

- A `ConsoleAppender`
- A `DateAndSizeRollingFileAppender`

Once the log manager is initialized, you can retrieve a logger that will be the logging API entry point:

```csharp
var log = LogManager.GetLogger(typeof(Program));
log.DebugFormat("Hello {0}", "world");
```

In order to meet the zero allocation constraint, the API had to be a little different from those we usually meet in more common logging libraries. However, we managed to provide two different API styles that should be pretty straightforward to use :

- A `StringBuilder` like API:

```csharp
log.Info()
   .Append("Tomorrow (")
   .Append(tomorrow)
   .Append(") will occur in ")
   .Append(numberOfSecondsUntilTomorrow)
   .Append(" seconds")
   .Log();
```

- A more classic, string format like API:

```csharp
log.InfoFormat("Tomorrow ({0}) will occur in {1} seconds", tomorrow, numberOfSecondsUntilTomorrow);
```

Both APIs can be used in a zero allocation fashion, but not all formatting options are currently supported (notably for DateTimes and TimeSpans).

### Structured Data
ZeroLog supports appending structured data (formatted as JSON) to log messages.

Structured data can be appended by calling AppendKeyValue, like so:

```csharp
log.Info()
   .Append("Tomorrow is another day.")
   .AppendKeyValue("NumSecondsUntilTomorrow", numberOfSecondsUntilTomorrow)
   .Log();
```


## Configuration

Zero log supports hierarchical loggers and can be configured using a Json configuration file: 

```csharp
JsonConfigurator.ConfigureAndWatch("ZeroLog.json");
```

Please see [ZeroLog.json](ZeroLog.json)
There is three part to the configuration file:

### Appenders

You have to configure a set of appenders (*output channel*) that can be used by the loggers.

 - **Name** is a unique identifier used by loggers for reference
 - **AppenderTypeName** is the full type name used to instanciate the appender
 - **AppenderJsonConfig** is any additional parameters used by the appender

### Loggers

When ``GetLogger("Foo.Something.Item")`` is called, it will try to find the best matching configuration using a hierarchical namespace-like mode.
If ``Foo.Something`` is configured, but ``Foo.Something.Item`` is not, it will use ``Foo.Something`` configuration.

A logger configuration is composed of a *Name*, a *Level*, a list of *AppenderReferences* (name of appenders to use) and optionaly a boolean *IncludeParentAppenders*.  

 - **Name** is used for hierarchical matching
 - **Level** is the minimal level the logger will work on
 - **AppenderReferences** is a list of appenders names the logger will use
 - **IncludeParentAppenders** (optional) will, if set to true, copy the parent appenders into the current logger
 - **LogEventPoolExhaustionStrategy** (optional) is used to specify what to do when the log event queue is full
 - **LogEventArgumentExhaustionStrategy** (optional) is used to specify what to do when the maximum number of `Append` calls is exceeded

### RootLogger

The root logger is the default logger. If a GetLogger is called on an unconfigured namespace, it will fallback to the root logger.
Same parameters as other loggers.

### Log Event Pool Exhaustion Strategy

There are currently three strategies to handle a full queue scenario:

- **DropLogMessageAndNotifyAppenders** (default) Drop the log message and log an error instead
- **DropLogMessage** Forget about the message
- **WaitForLogEvent** Block until it's possible to log

### Log Event Argument Exhaustion Strategy

If the maximum number of `Append` calls is exceeded, the available strategies which handle that are:

- **TruncateMessage** (default) The message is truncated, and a customizable suffix is appended
- **Allocate** Allocate more space for the next argument

### Queue and message size

These values can be configured at the root of the json configuration file:

- **LogEventQueueSize** (default: `1024`) Count of pooled log events. A log event is acquired from the pool on demand, and released by the logging thread.
- **LogEventBufferSize** (default: `128`) The size of the buffer used to serialize log event arguments. Once exceeded, the message is truncated. All `Append` calls use a few bytes, except for `AppendAsciiString` which copies the whole string into the buffer.
- **LogEventArgumentCapacity** (default: `32`) The maximum number of `Append` calls that can be made for a log event. Additional calls will behave based on the configured `LogEventArgumentExhaustionStrategy`.

### Global Configuration

Some settings can be set globally on the `LogManager.Config` object:

- **LazyRegisterEnums** (default: `false`) Automatically registers an enum type when first logged. This causes some allocations. Use `LogManager.RegisterEnum` when automatic registration is disabled.
- **FlushAppenders** (default: `true`) Automatically flushes appenders when there is a pause in the log event stream.
- **NullDisplayString** (default: `"null"`) The string which should be logged instead of a `null` value
- **TruncatedMessageSuffix** (default: `" [TRUNCATED]"`) The string which is appended to a message when it is truncated
- **JsonSeparator** (default: `"  ~~ "`) The string which is appended before structured data in log messages (when structured data is present).

## What's next

 Even if ZeroLog is still a very young project, you can begin to use it from now on. However, a lot of things still need to be added:

 - More appenders
 - Multiple logging (formatting and appending) threads
 - Better layout pattern
 - Support of standard formatting options (some already available; some modifiers not supported for DateTime and TimeSpan)
 - XML code documentation for the public API
    
