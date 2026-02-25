namespace LDS.Core;

public abstract class Parameter(string name, string path) {
    public string Name { get; } = name;
    public string Address { get; } = path;
    public abstract T GetValue<T>();
}

public class FloatParameter(string name, string path, float value) : Parameter(name, path)
{
    public float Value { get; set; } = value;
    public override T GetValue<T>() => Value is T typeValue ? typeValue : throw new NotImplementedException();
}

public class IntParameter(string name, string path, int value) : Parameter(name, path) {
    public int Value { get; set; } = value;
    public override T GetValue<T>() => Value is T typeValue ? typeValue : throw new NotImplementedException();
}

public class BoolParameter(string name, string path, bool value) : Parameter(name, path)
{
    public bool Value { get; set; } = value;
    public override T GetValue<T>() => Value is T typeValue ? typeValue : throw new NotImplementedException();
}