# DeliveryApp Admin Dashboard — .NET 8 MVC

## Project Structure
```
DeliveryAdmin/
├── Controllers/
│   ├── AuthController.cs        — Login / Logout
│   ├── DashboardController.cs   — Overview & KPIs
│   ├── OrdersController.cs      — Orders + Status updates
│   ├── DriversController.cs     — Drivers + Verify
│   ├── RestaurantsController.cs — Full CRUD
│   ├── ProductsController.cs    — Full CRUD
│   ├── CategoriesController.cs  — Full CRUD
│   ├── CustomersController.cs   — Users list
│   ├── PaymentsController.cs    — Payment records
│   └── RatingsController.cs     — Customer ratings
├── Models/
│   └── ApiModels.cs             — All DTOs matching the API
├── Services/
│   └── ApiService.cs            — HTTP client wrapping all API calls
├── Views/
│   ├── Shared/_Layout.cshtml    — Dark sidebar layout
│   ├── Auth/Login.cshtml
│   ├── Dashboard/Index.cshtml
│   ├── Orders/{Index,Details}
│   ├── Drivers/Index
│   ├── Restaurants/{Index,Details,Create,Edit}
│   ├── Products/{Index,Create,Edit}
│   ├── Categories/{Index,Create,Edit}
│   ├── Customers/Index
│   ├── Payments/Index
│   └── Ratings/Index
└── wwwroot/css/admin.css        — Full dark theme CSS
```

## API
- Base URL: `https://deliveryappapi.runasp.net/api`
- Authentication: JWT Bearer (stored in Session)

## How to Run
```bash
cd DeliveryAdmin
dotnet run
# Visit: https://localhost:5001
```

## How to Deploy
```bash
dotnet publish -c Release -o ./publish
# Deploy ./publish folder to your server
```

## Pages & Features
| Page        | Features |
|-------------|----------|
| Dashboard   | KPI cards, bar chart, donut chart, recent orders |
| Orders      | Pipeline view, filter by status, status transitions |
| Order Detail| Full order info, items, driver, payment, status actions |
| Drivers     | Cards + table, verify driver, filter online/offline |
| Restaurants | List, create, edit, open/close toggle |
| Rest. Detail| Info + categories management |
| Categories  | Full CRUD per restaurant |
| Products    | Full CRUD, enable/disable, delete, filter by restaurant |
| Customers   | Users list, filter by role |
| Payments    | Payment records, revenue stats |
| Ratings     | Customer ratings table |
