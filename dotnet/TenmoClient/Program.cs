using System;
using System.Collections.Generic;
using System.Net.Http;
using TenmoClient.Data;

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
                    Console.WriteLine(api.GetBalance());
                }
                else if (menuSelection == 2)
                {
                    List<Transfer> list = api.ShowTransfers();

                    /*
                     *  -------------------------------------------
                        Transfers
                        ID          From/To                 Amount
                        -------------------------------------------
                    */
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("Transfers");
                    Console.WriteLine("ID\tFrom/To\t\tAmount");
                    Console.WriteLine("-------------------------------------------");


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


                            Console.WriteLine(item.TransferId + "\t" + transferDetail + "\t" + item.Amount);
                        //(item.TransferTypeId == 1 ? (item.AccountFromName != "eric" ? "From: " + item.AccountFromName : "From: " + item.AccountToName) : (item.AccountFromName != "eric" ? "To: " + item.AccountFromName : "To: " + item.AccountToName))
                        
                    }
                    Console.Write("\nEnter Transfer ID to see more details: ");
                    int transferId = int.Parse(Console.ReadLine());
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
                    Console.WriteLine("\tId: " + transfer.TransferId + "\n\tFrom: " + transfer.AccountFromName + 
                                      "\n\tTo: " + transfer.AccountToName + "\n\tType: " + transfer.TransferTypeId + 
                                      "\n\tStatus: " + transfer.TransferStatusName + "\n\tAmount: " + transfer.Amount);

                }
                else if (menuSelection == 3)
                {

                }
                else if (menuSelection == 4)
                {

                    //Get list of users
                    List<User> users =  api.GetUsers();

                    string recipient = MenuSelectionOptions(users);
                    User recipientUser = api.GetUser(recipient);

                    Console.Write("Please enter the amount to transfer: ");
                    decimal amountToSend = decimal.Parse(Console.ReadLine());

                    api.SendTeBucks(amountToSend, recipientUser.UserId);
                    //input user to send bucks to
                    //run transfer method
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

        private static string MenuSelectionOptions(List<User> users)
        {

            int i = 1;

            Console.WriteLine("\nPlease select who you would like to transfer to: \n");
            foreach (User user in users)
            {
                Console.WriteLine("\t" + i++ + ". " + user.Username);
            }

            int selecteduser =int.Parse( Console.ReadLine());
            return users[selecteduser - 1].Username;
        }

        //Transfer fund method is incorrectly 
    }
}
