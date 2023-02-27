using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Microsoft.Data.SqlClient;
using ABCBankingWebApp.Models;

namespace ABCBankingWebApp.Data
{
    public class FundTransferDAL
    {
        private readonly IConfiguration Configuration;
        private readonly string connectionString;

        public FundTransferDAL(IConfiguration configuration)
        {
            Configuration = configuration;
            connectionString = Configuration["ConnectionStrings:ABCBankDB"];
        }

        public IEnumerable<FundTransfer> GetFundTransfers()
        {
            List<FundTransfer> fundTransfers = new List<FundTransfer>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("Select * from FundTransfer", con);
                cmd.CommandType = CommandType.Text;

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    FundTransfer fundtransfer = new FundTransfer();

                    fundtransfer.ID = Convert.ToInt32(rdr["ID"]);
                    fundtransfer.AccountFrom = Convert.ToInt32(rdr["AccountFrom"]);
                    fundtransfer.AccountTo = Convert.ToInt32(rdr["AccountTo"]);
                    fundtransfer.TransactionDate = Convert.ToDateTime(rdr["TransactionDate"]);
                    fundtransfer.Amount = decimal.Parse(rdr["Amount"].ToString());
                    fundtransfer.Note = rdr["Note"].ToString();
                    fundTransfers.Add(fundtransfer);
                }
                con.Close();
            }
            return fundTransfers;
        }


        public IEnumerable<FundTransfer> GetFundTransfers(string search)
        {
            List<FundTransfer> fundTransfers = new List<FundTransfer>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                //using inline command and params
                SqlCommand cmd = new SqlCommand("Select * from FundTransfer where Note like @searchparam ", con);
                cmd.CommandType = CommandType.Text;


                cmd.Parameters.AddWithValue("@searchparam", search);

                //using SP
                /*
                SqlCommand cmd = new SqlCommand(con);
                cmd.CommandText = "get_FundTransfers";
                cmd.Parameters.AddWithValue("@searchparam", search);
                */

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    FundTransfer fundtransfer = new FundTransfer();

                    fundtransfer.ID = Convert.ToInt32(rdr["ID"]);
                    fundtransfer.AccountFrom = Convert.ToInt32(rdr["AccountFrom"]);
                    fundtransfer.AccountTo = Convert.ToInt32(rdr["AccountTo"]);
                    fundtransfer.TransactionDate = Convert.ToDateTime(rdr["TransactionDate"]);
                    fundtransfer.Amount = decimal.Parse(rdr["Amount"].ToString());
                    fundtransfer.Note = rdr["Note"].ToString();
                    fundTransfers.Add(fundtransfer);
                }
                con.Close();
            }
            return fundTransfers;
        }

    }
}