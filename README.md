# ZeroLog

ZeroLog is a **zero-allocation .NET logging library**. It uses the excellent formatting library [StringFormatter](https://github.com/MikePopoloski/StringFormatter).

  It provides basic logging capabilities to be used in latence-sensitive applications, where garbage collections are undesirable. ZeroLog can be used in a complete zero-allocation manner, meaning that after the initialization phase, it will not allocate any managed object on the heap, thus preventing any GC from being triggered.
  
  Since ZeroLog does not aim to replace any existing logging libraries in any kind of application, it won't try to compete on feature set level with more pre-eminent projets like log4net or NLog for example. The focus will remain on performance and allocation free aspects.
  
## Disclaimer

>The project is not production ready yet; it's still a work in progress. However, you can already obtain it via Nuget if you want to give it a try. The package is available here:

>https://www.nuget.org/packages/ZeroLog/

## Internal design
 
 ZeroLog was designed to meet two main objectives:

  - Being a **zero allocation library**
  - Doing **as little work as possible in calling threads**

The second goal implies a major design choice: the actual logging is completely asynchronous. It means that writing messages to the appenders obviously occurs in a background thread, but also that *all formatting operations are delayed to be performed just before the appending*. **No formatting occurs in the calling thread**; the log data is merely mashalled to the background logging thread in the more efficient way possible.

 Internally, each logging call data (context, log messages, arguments, etc.) will be serialized to a pooled log event, before being enqueued in a concurrent data structure the background logging thread consumes. The logging thread will then format the log messages and append them to the configured appenders.

## Getting started

Before using ZeroLog, you need to initialize the `LogManager`:

```csharp
LogManager.Initialize(new[] { new ConsoleAppender() });
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
log.Info().Append("Tomorrow (")
          .Append(tomorrow)
          .Append(") will occur in ")
          .Append(numberOfSecondsUntilTomorrow)
          .Append(" seconds").Log();
```

- A more classic, string format like API:

```csharp
log.InfoFormat("Tomorrow ({0}) will occur in {1} seconds", tomorrow, numberOfSecondsUntilTomorrow);
```

Both APIs can be used in a zero allocation fashion, but not all formatting options are currently supported (notably for DateTimes and TimeSpans).

## What's next

 Even if ZeroLog is still a very young project, you can begin to use it from now on. However, a lot of things still need to be added:

 - Configuration files
 - Dynamic log level configuration
 - More appenders
 - Multiple logging (formatting and appending) threads
 - Layout pattern (basic support already available in `DateAndSizeRollingFileAppender`)
 - Support of standard formatting options (some already available; some modifiers not supported for DateTime and TimeSpan) 
 - Support for `Exception` arguments
 - Support for `byte[]` arguments
 - Hierarchical loggers
 - XML code documentation for the public API
    
