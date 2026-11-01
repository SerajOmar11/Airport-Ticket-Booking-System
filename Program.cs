using AirportTicketBookingSystem.Application;
using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Services;

string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

string csvPath = Path.Combine(dataDirectory, "flights.csv");

string flightPath = Path.Combine(dataDirectory, "flights.json");
string bookingPath = Path.Combine(dataDirectory, "bookings.json");
string passengerPath = Path.Combine(dataDirectory, "passengers.json");

StorageService<Flight> flightStorage = new();
StorageService<Booking> bookingStorage = new();
StorageService<Passenger> passengerStorage = new();

FlightService flightService = new(
    flightStorage,
    bookingStorage,
    flightPath,
    bookingPath
);

BookingService bookingService = new(
    bookingStorage,
    passengerStorage,
    flightService,
    bookingPath,
    passengerPath
);

CsvFlightParser csvParser = new();
FlightValidator flightValidator = new();

FlightImportService importService = new(
    csvParser,
    flightValidator,
    flightStorage,
    flightPath
);

PassengerMenu passengerMenu = new(
    bookingService,
    flightService,
    passengerStorage,
    passengerPath
);

ManagerMenu managerMenu = new(
    importService,
    bookingService,
    flightValidator,
    csvPath
);

MainMenu mainMenu = new(
    passengerMenu,
    managerMenu
);

mainMenu.Show();