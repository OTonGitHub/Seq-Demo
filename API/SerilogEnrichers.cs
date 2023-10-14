using Serilog.Core;
using Serilog.Events;

namespace API.LogEventEnrichers;

public class ThreadIDEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                "ThreadID",
                Thread.CurrentThread.ManagedThreadId
            )
        );
    }

    // Session Management Enricher
}
