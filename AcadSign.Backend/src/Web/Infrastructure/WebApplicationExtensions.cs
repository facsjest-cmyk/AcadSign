using System.Reflection;

namespace AcadSign.Backend.Web.Infrastructure;

public static class WebApplicationExtensions
{
    private static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GroupName ?? group.GetType().Name;
        var groupSlug = groupName.ToLowerInvariant();

        var prefix = groupName.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
            ? "/connect"
            : $"/api/v1/{groupSlug}";

        return app
            .MapGroup(prefix)
            .WithTags(groupName);
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app.MapGroup(instance));
            }
        }

        return app;
    }
}
