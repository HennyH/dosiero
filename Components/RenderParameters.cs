using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Dosiero.Components;

public sealed class RenderParameters : IComponent
{
    [Parameter(CaptureUnmatchedValues = true)]
    public required Dictionary<string, object?> Parameters { get; set; }

    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    public void Attach(RenderHandle renderHandle)
    {
        renderHandle.Render(builder => Render(builder));
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        return Task.CompletedTask;
    }

    private void Render(RenderTreeBuilder builder, (string Name, IRenderParameter Parameter)[]? parameters = null, int index = 0)
    {
        parameters ??= [.. Parameters
            .Where(kvp => kvp.Value is IRenderParameter)
            .Select(kvp => (Name: kvp.Key, Parameter: (IRenderParameter)kvp.Value!))];

        if (index >= parameters.Length)
        {
            builder.AddContent(0, ChildContent);
        }
        else
        {
            var (name, parameter) = parameters[index];

            builder.OpenComponent(0, typeof(CascadingValue<>).MakeGenericType(parameter.Type));
            builder.AddComponentParameter(1, nameof(CascadingValue<>.Name), name);
            builder.AddComponentParameter(2, nameof(CascadingValue<>.Value), parameter.Value);
            builder.AddComponentParameter(3, nameof(CascadingValue<>.IsFixed), true);
            builder.AddComponentParameter(4, nameof(CascadingValue<>.ChildContent), (RenderFragment)(builder => Render(builder, parameters, index: index + 1)));
            builder.CloseComponent();
        }
    }
}

public interface IRenderParameter
{
    public string Name { get; }

    public object Value { get; }

    public Type Type { get; }
}

public record RenderParameter<T>(string Name, T Value) : IRenderParameter
    where T : notnull
{
    public Type Type => typeof(T);

    object IRenderParameter.Value => Value;
}

public static class RenderParameter
{
    public static RenderParameter<T> For<T>(string name, T value)
        where T : notnull => new(name, value);
}

public static class RenderParameterExtensions
{
    public static ParameterView ToParameterView(this IEnumerable<IRenderParameter> parameters)
        => ParameterView.FromDictionary(parameters.ToDictionary(p => p.Name, p => (object?)p));
}