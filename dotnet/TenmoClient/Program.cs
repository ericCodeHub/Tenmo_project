﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using TenmoClient.Data;
using System.Linq;

namespace TenmoClient
{
    class Program
    {
        private static readonly ConsoleService consoleService = new ConsoleService();
        private static readonly AuthService authService = new AuthService();
        private static readonly ApiService api = new ApiService();

        static void Main(string[] args)
        {
            Run();
        }
        private static void Run()
        {
            int loginRegister = -1;
            while (loginRegister != 1 && loginRegister != 2)
            {
                Console.WriteLine("Welcome to TEnmo!");
                Console.WriteLine("1: Login");
                Console.WriteLine("2: Register");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out loginRegister))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (loginRegister == 1)
                {
                    while (!UserService.IsLoggedIn()) //will keep looping until user is logged in
                    {
                        LoginUser loginUser = consoleService.PromptForLogin();
                        API_User user = authService.Login(loginUser);
                        if (user != null)
                        {
                            UserService.SetLogin(user);
                            
                        }
                    }
                }
                else if (loginRegister == 2)
                {
                    bool isRegistered = false;
                    while (!isRegistered) //will keep looping until user is registered
                    {
                        LoginUser registerUser = consoleService.PromptForLogin();
                        isRegistered = authService.Register(registerUser);
                        if (isRegistered)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Registration successful. You can now log in.");
                            loginRegister = -1; //reset outer loop to allow choice for login
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }

            MenuSelection();
        }

        private static void MenuSelection()
        {
            int menuSelection = -1;
            while (menuSelection != 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (menuSelection == 1)
                {
                    
                    Console.WriteLine(string.Format("{0:C}", api.GetBalance()));
                }
                else if (menuSelection == 2)
                {
                    List<Transfer> list = api.ShowTransfers();

                    
                    
                    int transferId = -1;
                    while (transferId < 0)
                    {
                        Console.WriteLine(ShowUserTransfers(list));
                        Console.WriteLine("\nEnter Transfer ID to see more details or 0 to return to main Menu: ");
                        try
                        {
                            transferId = int.Parse(Console.ReadLine());
                        }
                        catch
                        {
                            transferId = -1;
                        }
                        if (transferId == 0)
                        {
                            menuSelection = -1;
                        }
                        else if (!list.Any(x => x.TransferId == transferId))
                        {
                            transferId = -1;

                        }
                        else
                        {
                            Transfer transfer = api.ShowTransfer(transferId);

                            /*--------------------------------------------
                    Transfer Details
                    --------------------------------------------
                        Id: 23
                        From: Bernice
                        To: Me Myselfandi
                        Type: Send
                        Status: Approved
                        Amount: $903.14*/

                            Console.WriteLine("\n--------------------------------------------");
                            Console.WriteLine("Transfer Details");
                            Console.WriteLine("--------------------------------------------");
                            Console.WriteLine($"\tId: {transfer.TransferId}\n\tFrom: {transfer.AccountFromName}" +
                                              $"\n\tTo: {transfer.AccountToName}\n\tType: {transfer.TransferTypeId}" +
                                              $"\n\tStatus: {transfer.TransferStatusName}\n\tAmount: {transfer.Amount:C}");
                            transferId = -1;
                        }
                    }




                }
                else if (menuSelection == 3)
                {

                }
                else if (menuSelection == 4)
                {

                    //Get list of users
                    Console.WriteLine(UserService.GetUserName());
                    List<User> users =  api.GetUsers();
                    //remove current user from list of potential recipients
                    users.RemoveAt(UserService.GetUserId() - 1);

                    int recipient = MenuSelectionOptions(users);
                    if (recipient == 0)
                    {
                        Console.WriteLine("transfer canceled");
                        menuSelection = -1;
                    }
                    else
                    {
                        User recipientUser = api.GetUser(users[recipient-1].Username);

                        Console.Write("Please enter the amount to transfer: ");
                        decimal amountToSend = decimal.Parse(Console.ReadLine());

                        api.SendTeBucks(amountToSend, recipientUser.UserId);
                        //input user to send bucks to
                        //run transfer method
                    }
                }
                else if (menuSelection == 5)
                {

                }
                else if (menuSelection == 6)
                {
                    Console.WriteLine("");
                    UserService.SetLogin(new API_User()); //wipe out previous login info
                    Run(); //return to entry point
                }
                else
                {
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                }
            }
        }

        private static string ShowUserTransfers(List<Transfer> list)
        {
            /*
                -------------------------------------------
                Transfers
                ID          From/To                 Amount
                -------------------------------------------
            */
            string userListOfTransfers;

            userListOfTransfers = "-------------------------------------------\n";
            userListOfTransfers += "Transfers\n";
            userListOfTransfers += $"ID\t{"From/To",-20}Amount\n";
            userListOfTransfers += "-------------------------------------------\n";


            foreach (Transfer item in list)
            {

                string transferDetail = "";
                if (item.TransferTypeId == 1)
                {
                    if (item.AccountFrom == UserService.GetUserId())
                    {
                        transferDetail = "To: " + item.AccountToName;
                    }
                    else
                    {
                        transferDetail = "From: " + item.AccountFromName;
                    };
                }
                else
                {
                    if (item.TransferTypeId == 2)
                    {
                        if (item.AccountTo == UserService.GetUserId())
                        {
                            transferDetail = "To: " + item.AccountToName;
                        }
                        else
                        {
                            transferDetail = "From: " + item.AccountFromName;
                        }
                    }
                }

                userListOfTransfers += $"{ item.TransferId}\t{ transferDetail,-20}{item.Amount:C}\n";
                //Console.WriteLine("${ item.TransferId}\t{ transferDetail,20}{ item.Amount}");
                //(item.TransferTypeId == 1 ? (item.AccountFromName != "eric" ? "From: " + item.AccountFromName : "From: " + item.AccountToName) : (item.AccountFromName != "eric" ? "To: " + item.AccountFromName : "To: " + item.AccountToName))
                
            }
            return userListOfTransfers;
        }

        private static int MenuSelectionOptions(List<User> users)
        {

            int i = 1;
            int selectedUser = -1;

            while (selectedUser < 0 || selectedUser > users.Count)
            {
                Console.WriteLine("\nPlease select who you would like to transfer to: \n");
                foreach (User user in users)
                {
                    Console.WriteLine("\t" + i++ + ". " + user.Username);
                }
                selectedUser = int.Parse(Console.ReadLine());
                i = 1;
            }
            if (selectedUser == 0)
            {
                return 0;
            } else
            {
                return users[selectedUser - 1].UserId;
            }
            
        }

        //Transfer fund method is incorrectly 
    }
}
