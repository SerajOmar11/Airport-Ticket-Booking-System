namespace AirportTicketBookingSystem.Application;

public class MainMenu
{
    private readonly PassengerMenu _passengerMenu;
    private readonly ManagerMenu _managerMenu;

    public MainMenu(PassengerMenu passengerMenu,ManagerMenu managerMenu)
    {
        _passengerMenu = passengerMenu
                         ?? throw new ArgumentNullException(nameof(passengerMenu));

        _managerMenu = managerMenu
                       ?? throw new ArgumentNullException(nameof(managerMenu));
    }

    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=================================");
            Console.WriteLine(" AIRPORT TICKET BOOKING SYSTEM");
            Console.WriteLine("=================================");
            Console.WriteLine("1. Passenger");
            Console.WriteLine("2. Manager");
            Console.WriteLine("0. Exit");
            Console.WriteLine();

            int choice = ConsoleHelper.ReadInt("Choose: ", 0, 2);

            try
            {
                switch (choice)
                {
                    case 1:
                        _passengerMenu.Show();
                        break;

                    case 2:
                        _managerMenu.Show();
                        break;

                    case 0:
                        Console.WriteLine();
                        Console.WriteLine("Goodbye!");
                        return;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.ShowError(ex);
                ConsoleHelper.Pause();
            }
        }
    }
}