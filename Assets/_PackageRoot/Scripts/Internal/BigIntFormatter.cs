using Sirenix.Serialization;
using BigInt = System.Numerics.BigInteger;

[assembly: RegisterFormatter(typeof(Project.Store.Serialization.BigIntFormatter))]
namespace Project.Store.Serialization
{
    public class BigIntFormatter : MinimalBaseFormatter<BigInt>
    {
        private static readonly Serializer<byte[]> Serializer = Sirenix.Serialization.Serializer.Get<byte[]>();

        protected override void Read(ref BigInt value, IDataReader reader)
        {
            var bytes = BigIntFormatter.Serializer.ReadValue(reader);
			if (bytes == null || bytes.Length == 0)
			{
				value = 0;
			}
			else
			{
				value = new BigInt(bytes);
			}
        } 

        protected override void Write(ref BigInt value, IDataWriter writer)
        {
            var bytes = value.ToByteArray();
			BigIntFormatter.Serializer.WriteValue(bytes, writer);
        }
    }
}