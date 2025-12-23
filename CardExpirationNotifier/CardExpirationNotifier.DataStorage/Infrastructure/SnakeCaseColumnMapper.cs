using System.Reflection;
using System.Text.RegularExpressions;
using Dapper;

namespace CardExpirationNotifier.DataStorage.Infrastructure;

public class SnakeCaseColumnMapper : SqlMapper.ITypeMap
{
    private readonly Type _type;

    public SnakeCaseColumnMapper(Type type)
    {
        _type = type;
    }

    public ConstructorInfo? FindConstructor(string[] names, Type[] types)
    {
        // Return the parameterless constructor
        return _type.GetConstructor(Type.EmptyTypes);
    }

    public ConstructorInfo? FindExplicitConstructor()
    {
        // Return the parameterless constructor
        return _type.GetConstructor(Type.EmptyTypes);
    }

    public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
    {
        return null;
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        var property = _type.GetProperties()
            .FirstOrDefault(p => string.Equals(ToSnakeCase(p.Name), columnName, StringComparison.OrdinalIgnoreCase));

        if (property != null)
        {
            return new SimpleMemberMap(columnName, property);
        }

        return null;
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return Regex.Replace(input, "([a-z])([A-Z])", "$1_$2").ToLower();
    }

    private class SimpleMemberMap : SqlMapper.IMemberMap
    {
        private readonly PropertyInfo _property;

        public SimpleMemberMap(string columnName, PropertyInfo property)
        {
            ColumnName = columnName;
            _property = property;
            MemberType = property.PropertyType;
        }

        public string ColumnName { get; }
        public Type MemberType { get; }
        public PropertyInfo Property => _property;
        public FieldInfo? Field => null;
        public ParameterInfo? Parameter => null;
    }
}
