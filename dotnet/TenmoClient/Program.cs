using System;
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
                else if (menuSelection == 1)//show user balance
                {
                    
                    Console.WriteLine(string.Format("{0:C}", api.GetBalance()));
                }
                else if (menuSelection == 2)//show logged in users transfers
                {
                    List<Transfer> list = api.ShowTransfers();
                    string menuMessage = "\nEnter Transfer ID to see more details or 0 to return to main Menu: ";
                    menuSelection = ShowTransferDetails(menuSelection, list, menuMessage);

                }
                else if (menuSelection == 3)//view logged in users pending requests
                {
                    //grab lists of pending requests
                    //use show transfers model but only show pending transfers instead of all transfers
                    List<Transfer> list = api.ShowPendingRequests();

                    
                    string menuMessage = "Please enter transfer ID to approve/reject (0 to cancel): ";
                    menuSelection = ShowTransferDetails(menuSelection, list, menuMessage);

                }
                else if (menuSelection == 4)//send money to other users
                {


                    /*Console.WriteLine(UserService.GetUserName()); /* this is debugger code that can be deleted once
                    option 5 is completed */
                    List<User> users = api.GetUsers();//Get list of users

                    users.RemoveAt(UserService.GetUserId() - 1); //remove current user from list of potential recipients
                    string menuMessage = "Please select who you would like to transfer funds to or enter 0 to cancel the transfer: ";
                    int recipient = MenuSelectionOptions(menuMessage, users);
                    CreateNewTransfer(ref menuSelection, users, ref recipient);
                }
                else if (menuSelection == 5)//request money from other members
                {
                    List<User> users = api.GetUsers();
                    //remove current user from list of potential recipients
                    users.RemoveAt(UserService.GetUserId() - 1);
                    string menuMessage = @"Please select who you would like to request funds from or " +
                                           "enter 0 to cancel the request: ";
                    int sender = MenuSelectionOptions(menuMessage, users);

                    if (sender == 0)//cancels/exits out of current operation
                    {
                        Console.WriteLine("transfer canceled");
                        menuSelection = -1;
                    }
                    else
                    {
                        
                        CreateTransferRequest(ref sender);

                    }

                }
                else if (menuSelection == 6)
                {
                    Console.WriteLine("");
                    UserService.SetLogin(new API_User()); //wipe out previous login info
                    Run(); //return to entry point
                }
                else if (menuSelection == 0)
                {
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                } else
                {
                    Console.WriteLine("Please enter a valid menu selection");
                    menuSelection = -1;
                }
            }
        }

        private static void CreateTransferRequest(ref int sender)
        {
            decimal amountToRequest;
            do
            {
                Console.Write("Please enter the amount you are requesting (0 to cancel): ");
                amountToRequest = decimal.Parse(Console.ReadLine());

                if (amountToRequest < 0)
                {
                    Console.WriteLine("please enter an amount above {0:C}");
                    amountToRequest = -1;
                }
                else if (amountToRequest == 0)
                {
                    Console.WriteLine("transaction canceled");
                    sender = 0;
                }
                else
                {
                    api.RequestTeBucks(amountToRequest, sender);//create transfer request
                }

            } while (amountToRequest < 0);
            //return amountToRequest;
        }

        private static void CreateNewTransfer(ref int menuSelection, List<User> users, ref int recipient)
        {
            int recipientId = recipient;
            if (recipient == 0)
            {
                Console.WriteLine("transfer canceled");
                menuSelection = -1;
            }
            else
            {
                //User recipientUser = api.GetUser(users[recipient].Username);

                string recipientUserName = users[users.FindIndex(x => x.UserId == recipientId)].Username;
                //Console.WriteLine(recipientUser);

                decimal amountToSend;
                do
                {

                    Console.Write("Please enter the amount to transfer (0 to cancel): ");
                    amountToSend = decimal.Parse(Console.ReadLine());

                    if (amountToSend == 0)
                    {
                        Console.WriteLine("transaction canceled");
                        recipient = 0;
                    }
                    else if (api.GetBalance() > amountToSend) //check that there is enough money in the account
                    {
                        api.SendTeBucks(amountToSend, recipient);//transfer funds
                        Console.WriteLine($"{amountToSend:C} sent to {recipientUserName}");

                    }
                    else
                    {
                        Console.WriteLine("insufficient funds");
                    }
                } while (!(api.GetBalance() > amountToSend));

            }
        }

        private static int ShowTransferDetails(int menuSelection, List<Transfer> list, string message)
        {
            int transferId = -1;
            while (transferId < 0)
            {
                Console.WriteLine(ShowUserTransfers(list));
                Console.WriteLine(message);
                try
                {
                    transferId = int.Parse(Console.ReadLine());
                }
                catch//if a "non-transfer id" is entered, do this
                {
                    transferId = -1;
                }
                if (transferId == 0)//user selects to exit out
                {
                    menuSelection = -1;
                }
                else if (!list.Any(x => x.TransferId == transferId))//checks for valid entry
                {
                    transferId = -1;
                }
                else//if transfer exists
                {
                    if (menuSelection == 2)//show all transfers
                    {
                        Console.WriteLine(TransferDetails(transferId));
                    } else if (menuSelection == 3)//show pending requests
                    {
                        Console.WriteLine(PendingRequestDetails(transferId));
                        int userSelection = int.Parse(Console.ReadLine());//transfer status
                        PendingRequestUpdate(ref menuSelection, list, transferId, ref userSelection);
                    }

                    transferId = -1;
                }
            }

            return menuSelection;
        }

        private static void PendingRequestUpdate(ref int menuSelection, List<Transfer> list, int transferId, ref int userSelection)
        {
            do
            {
                if (userSelection == 0)
                {
                    menuSelection = 3;
                }
                else if (userSelection == 1)
                {
                    Transfer transfer = api.ShowTransfer(transferId);
                    //change transfer status to approved (2)
                    if (api.GetBalance() > transfer.Amount)//make sure user has funds to approve
                    {
                        api.UpdateTransferStatus(transferId, 2);
                        list.RemoveAt(list.FindIndex(x => x.TransferId == transferId));
                        //string recipientUserName = users[users.FindIndex(x => x.UserId == recipient)].Username;
                    }
                    else
                    {
                        Console.WriteLine("insufficient funds; please add funds before approving");
                        menuSelection = 3;//show list of transfers again
                    }

                }
                else if (userSelection == 2)
                {
                    //change transfer status to rejected (1)
                    api.UpdateTransferStatus(transferId, 1);
                    list.RemoveAt(list.FindIndex(x => x.TransferId == transferId));
                    if (list.Count == 0) { menuSelection = 3; }
                }
                else
                {
                    Console.WriteLine("please enter a valid menu selection");
                    userSelection = -1;
                }
            } while (userSelection < 0);
        }

        private static string PendingRequestDetails(int transferId)
        {
            Transfer transfer = api.ShowTransfer(transferId);
            string requestInfo = $"requested transfer {transfer.TransferId} of {transfer.Amount:C} to {transfer.AccountToName}";
            string result = $"1:  Approve {requestInfo}\n2:  Reject {requestInfo}\n0:  Don't approve or reject" ;

            return result;

        }

        private static string TransferDetails(int transferId)
        {
            Transfer transfer = api.ShowTransfer(transferId);
            string result;
            /*--------------------------------------------
    Transfer Details
    --------------------------------------------
        Id: 23
        From: Bernice
        To: Me Myselfandi
        Type: Send
        Status: Approved
        Amount: $903.14*/

            result ="\n--------------------------------------------";
            result+="\nTransfer Details";
            result+="\n--------------------------------------------";
            result += $"\n\tId: {transfer.TransferId}\n\tFrom: {transfer.AccountFromName}" +
                    $"\n\tTo: {transfer.AccountToName}\n\tType: {transfer.TransferTypeId}" +
                    $"\n\tStatus: {transfer.TransferStatusName}\n\tAmount: {transfer.Amount:C}";
                    
            return result;
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
            userListOfTransfers += $"ID\t{"From/To",-25}Amount\n";
            userListOfTransfers += "-------------------------------------------\n";


            foreach (Transfer item in list)
            {

                string transferDetail = "";
                if (item.TransferTypeId == 2)//all sent transfers get a 2; requested funds are transfer type 1
                {
                    if (item.AccountFrom == UserService.GetUserId())
                    {
                        transferDetail = "Sent To: " + item.AccountToName;
                    }
                    else
                    {
                        transferDetail = "Sent From: " + item.AccountFromName;
                    };
                }
                else
                {
                    if (item.TransferTypeId == 1)//this condition represents requested funds both pending and approved
                    {
                        if (item.AccountTo == UserService.GetUserId())
                        {
                            transferDetail = "Req From: " + item.AccountFromName;
                        }
                        else
                        {
                            transferDetail = "Req By: " + item.AccountToName;
                        }
                    }
                }

                userListOfTransfers += $"{ item.TransferId}\t{ transferDetail,-25}{item.Amount:C}\n";
                //Console.WriteLine("${ item.TransferId}\t{ transferDetail,20}{ item.Amount}");
                //(item.TransferTypeId == 1 ? (item.AccountFromName != "eric" ? "From: " + item.AccountFromName : "From: " + item.AccountToName) : (item.AccountFromName != "eric" ? "To: " + item.AccountFromName : "To: " + item.AccountToName))
                
            }
            return userListOfTransfers;
        }

        private static int MenuSelectionOptions(string message, List<User> users)
        {

            int i = 1;
            int selectedUser = -1;

            while (selectedUser < 0 || selectedUser > users.Count)
            {
                Console.WriteLine($"\n{message}\n");
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
