using ABCBankingWebApp.Models;
using System;
using System.Runtime.Serialization;

namespace ABCBankingWebApp.Extensions
{
    public class LoanDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.Equals("ABCBankingWebApp.Models.Loan")){
                return typeof(Loan);
            }
            return null;
        }
    }
}