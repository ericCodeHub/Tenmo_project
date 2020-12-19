using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class UserSqlDAO : IUserDAO
    {
        private readonly string connectionString;
        const decimal startingBalance = 1000;
        public decimal CurrentBalance { get; set; }
        public decimal RecipientBalance { get; set; }

        public UserSqlDAO(string dbConnectionString)
        {
            CurrentBalance = startingBalance;
            connectionString = dbConnectionString;
        }

        public User GetUser(string userName)
        {
            User returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users WHERE username = @userName", conn);
                    cmd.Parameters.AddWithValue("@userName", userName);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        returnUser = GetUserFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }

        public List<User> GetUsers()
        {
            List<User> returnUsers = new List<User>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            User u = GetUserFromReader(reader);
                            returnUsers.Add(u);
                        }

                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUsers;
        }

        public User AddUser(string username, string password)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO users (username, password_hash, salt) VALUES (@username, @password_hash, @salt)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT @@IDENTITY", conn);
                    int userId = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand("INSERT INTO accounts (user_id, balance) VALUES (@userid, @startBalance)", conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    cmd.Parameters.AddWithValue("@startBalance", startingBalance);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return GetUser(username);
        }

        private User GetUserFromReader(SqlDataReader reader)
        {
            User u = new User()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
            };

            return u;
        }

        public decimal GetCurrentBalance(int userId)
        {
            decimal balance = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT balance FROM accounts WHERE user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    balance = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return balance;
        }

        public decimal TransferFunds(decimal transactionAmount, int currentUserId, int recipient)
        {


            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT balance FROM accounts WHERE user_id=@currentUserId", conn);
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);

                    CurrentBalance = Convert.ToDecimal(cmd.ExecuteScalar());

                    //CurrentBalance = CurrentBalance - transactionAmount;

                    cmd = new SqlCommand("UPDATE accounts SET balance = @CurrentBalance WHERE user_id = @currentUserId", conn);
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    cmd.Parameters.AddWithValue("@CurrentBalance", CurrentBalance -= transactionAmount);

                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT balance FROM accounts WHERE user_id= @recipient", conn);
                    cmd.Parameters.AddWithValue("@recipient", recipient);

                    RecipientBalance = Convert.ToDecimal(cmd.ExecuteScalar());

                    //CurrentBalance = CurrentBalance - transactionAmount;

                    cmd = new SqlCommand("UPDATE accounts SET balance = @RecipientBalance WHERE user_id = @recipient", conn);
                    cmd.Parameters.AddWithValue("@recipient", recipient);
                    cmd.Parameters.AddWithValue("@RecipientBalance", RecipientBalance += transactionAmount);

                    cmd.ExecuteNonQuery();

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return CurrentBalance;
        }
        public bool AddTransfer(decimal transferAmount, int transfer_type_id, int transfer_status_id, int account_from, int account_to)
        {
            int result;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) 
                                                    VALUES(@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount)", conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", transfer_type_id);
                    cmd.Parameters.AddWithValue("@transfer_status_id", transfer_status_id);
                    cmd.Parameters.AddWithValue("@account_from", account_from);
                    cmd.Parameters.AddWithValue("@account_to", account_to);
                    cmd.Parameters.AddWithValue("@amount", transferAmount);

                    result = cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
            return result > 0;
        }
        //#5 ---
        //Create a SQL method that INSERT's the transfer that was made. 
        //Transfers must have the:
        //Transfer type: (Request or Send)
        //Transfer Status: (Pending, Approved, Rejected)
        //Account from, Account to, and Amount transferred.
        //Append this to the end of the TransferFunds method to append table each time a transfer is made
        //Create a separate method to list all of the transfers made so far

        public List<Transfer> ShowUserTransfers(int currentUserId)
        {
                        
            List<Transfer> transfers = new List<Transfer>();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT transfer_id, transfer_type_id, ts.transfer_status_id, account_from, account_to, amount, u.user_id, 
                                                    u.username as accountFrom, (SELECT username FROM users WHERE user_id = account_to) as accountTo,
                                                    ts.transfer_status_desc
                                                    FROM transfers t
                                                    INNER JOIN accounts a ON t.account_from = a.account_id
                                                    INNER JOIN users u ON a.user_id = u.user_id
                                                    INNER JOIN transfer_statuses ts ON t.transfer_status_id = ts.transfer_status_id
                                                    WHERE account_from = @currentUserId OR account_to = @currentUserId", conn);
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Transfer t = GetTransferFromReader(reader);
                            transfers.Add(t);
                        }

                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfers;
        }
        
        private Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer t = new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]),
                TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]),
                AccountFrom = Convert.ToInt32(reader["account_from"]),
                AccountTo = Convert.ToInt32(reader["account_to"]),
                Amount = Convert.ToDecimal(reader["amount"]),
                AccountFromName = Convert.ToString(reader["accountFrom"]),
                AccountToName = Convert.ToString(reader["accountTo"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                TransferStatusName=Convert.ToString(reader["transfer_status_desc"])
                
            };

            return t;
        }
        public Transfer GetTransfer(int transferId)
        {
            Transfer transfer = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"SELECT transfer_id, transfer_type_id, ts.transfer_status_id, account_from, account_to, amount, u.user_id, 
                                                    u.username as accountFrom, (SELECT username FROM users WHERE user_id = account_to) as accountTo,
                                                    ts.transfer_status_desc
                                                    FROM transfers t
                                                    INNER JOIN accounts a ON t.account_from = a.account_id
                                                    INNER JOIN users u ON a.user_id = u.user_id
                                                    INNER JOIN transfer_statuses ts ON t.transfer_status_id = ts.transfer_status_id
                                                    WHERE transfer_id = @transferId", conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        transfer = GetTransferFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfer;
        }
    }
}

    