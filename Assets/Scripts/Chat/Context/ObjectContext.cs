using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using UnityEngine;

public enum ObjectType
{
    /** This object's context will automatically be added to the global context */
    Generic,
    /** Context for interactable NPCs */
    NPC
}

public interface IContext
{
    public string Type { get; }
    
    public void WriteJson(Utf8JsonWriter writer);
    
    public void WriteString(StringBuilder sb);
}

public class ObjectKinematicContext : IContext
{
    public string Type { get; } = nameof(ObjectKinematicContext);

    private readonly Transform _transform;
    private readonly Rigidbody _rigidbody;

    public ObjectKinematicContext(Transform transform, Rigidbody rigidbody)
    {
        _transform = transform;
        _rigidbody = rigidbody;
    }

    public void WriteJson(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        // Position
        {
            writer.WriteStartObject("Position");
            writer.WriteNumber("x", _transform.position.x);
            writer.WriteNumber("y", _transform.position.y);
            writer.WriteNumber("z", _transform.position.z);
            writer.WriteEndObject();
        }
        // Rotation
        {
            writer.WriteStartObject("Rotation");
            writer.WriteNumber("x", _transform.rotation.x);
            writer.WriteNumber("y", _transform.rotation.y);
            writer.WriteNumber("z", _transform.rotation.z);
            writer.WriteNumber("w", _transform.rotation.w);
            writer.WriteEndObject();
        }
        // Linear Velocity
        {
            writer.WriteStartObject("LinearVelocity");
            writer.WriteNumber("x", _rigidbody.linearVelocity.x);
            writer.WriteNumber("y", _rigidbody.linearVelocity.y);
            writer.WriteNumber("z", _rigidbody.linearVelocity.z);
            writer.WriteEndObject();
        }
        // Angular Velocity
        {
            writer.WriteStartObject("AngularVelocity");
            writer.WriteNumber("x", _rigidbody.angularVelocity.x);
            writer.WriteNumber("y", _rigidbody.angularVelocity.y);
            writer.WriteNumber("z", _rigidbody.angularVelocity.z);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    public void WriteString(StringBuilder sb)
    {
        sb.AppendLine($"Position: {_transform.position}");
        sb.AppendLine($"Rotation: {_transform.rotation}");
        sb.AppendLine($"Linear Velocity: {_rigidbody.linearVelocity}");
        sb.AppendLine($"Angular Velocity: {_rigidbody.angularVelocity}");
    }
}

public class ObjectContext : IContext
{
    public string Type { get; } = nameof(ObjectContext);
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [CanBeNull] public ObjectKinematicContext ObjectKinematic = null;

    [CanBeNull]
    public Dictionary<string, IContext> Other = null;

    public void WriteJson(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", Type);
        writer.WriteString("Name", Name);
        writer.WriteString("Description", Description);

        if (ObjectKinematic is not null)
        {
            writer.WritePropertyName("KinematicState");
            ObjectKinematic.WriteJson(writer);
        }
        else writer.WriteNull("KinematicState");
        writer.WriteEndObject();
    }

    public void WriteString(StringBuilder sb)
    {
        sb.AppendLine($"Type: {Type}");
        sb.AppendLine($"Name: {Name}");
        sb.AppendLine($"Description: {Description}");
        ObjectKinematic?.WriteString(sb);
    }
}

