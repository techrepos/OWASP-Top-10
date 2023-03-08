using ABCBankingWebAppAdmin.Models;
using System.Runtime.Serialization;

namespace ABCBankingWebAppAdmin.Extensions
{
    public class LoanDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.Equals("ABCBankingWebAppAdmin.Models.Loan"))
            {
                return typeof(Loan);
            }
            return null;
        }
    }
}