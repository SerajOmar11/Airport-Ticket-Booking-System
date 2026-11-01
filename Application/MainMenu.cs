namespace AirportTicketBookingSystem.Application;

public class MainMenu
{
    private readonly PassengerMenu _passengerMenu;
    private readonly ManagerMenu _managerMenu;

    public MainMenu(
        PassengerMenu passengerMenu,
        ManagerMenu managerMenu)
    {
        _passengerMenu = passengerMenu;
        _managerMenu = managerMenu;
    }

    public void Show()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("===== AIRPORT TICKET BOOKING SYSTEM =====");
            Console.WriteLine("1. Passenger");
            Console.WriteLine("2. Manager");
            Console.WriteLine("0. Exit");

            int choice = ConsoleHelper.ReadInt("Choose: ", 0, 2);

            switch (choice)
            {
                case 1:
                    _passengerMenu.Show();
                    break;
                case 2:
                    _managerMenu.Show();
                    break;
                case 0:
                    Console.WriteLine("Goodbye!");
                    return;
            }
        }
    }
}