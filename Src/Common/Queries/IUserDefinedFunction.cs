
namespace Alachisoft.NoSQL.Common.Queries
{
    public interface IUserDefinedFunction : IAggregation
    {
        void ApplyValue(params object[] value);
    }
}
