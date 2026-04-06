# PharmacyApp

A .NET 9 Web API app (PharmacyApp) that:

Serves a single-page UI from wwwroot (vanilla JavaScript modules — no npm build).
Stores data in JSON files under Data/medicines.json and Data/sales.json (created automatically if missing).
Medicine fields
Full name, notes, expiry date, quantity, price (rounded to 2 decimals on the server), brand.

UI behavior
Inventory tab: table with Full name, Brand, Expiry, Quantity, Price (notes are only on add/edit via the form, not in the grid).
Row colors: Red if expiry is less than 30 days from today (including expired). Yellow if quantity < 10 (only if the row is not already red).
Search: filters by medicine name (debounced) via GET /api/medicines?search=....
Add medicine: form to create records.
Sales: list of sales (date, medicine, qty, unit price, line total).
Record sale: choose medicine and quantity; stock is reduced and a sale line is appended to sales.json.
API
Method	Path	Purpose
GET
/api/medicines
List (optional ?search=name)
GET
/api/medicines/{id}
One medicine
POST
/api/medicines
Create
PUT
/api/medicines/{id}
Update
DELETE
/api/medicines/{id}
Delete
GET
/api/sales
List sales
POST
/api/sales
Record sale (medicineId, quantitySold)
How to run (Web app)
This project is a web app, so use:

cd "c:\Users\user\Documents\Pharmacy App\PharmacyApp"
dotnet run
Then open a browser to http://localhost:5076 (see Properties/launchSettings.json for ports). In Cursor, use Open Preview with that URL if you use preview.

HTTPS is also configured on https://localhost:7161 in the https profile if you run with that profile.
