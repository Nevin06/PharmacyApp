# PharmacyApp

A .NET 9 Web API app (**PharmacyApp**) that:

- Serves a single-page UI from `wwwroot` (vanilla JavaScript modules — no npm build)
- Stores data in JSON files under:
  - `Data/medicines.json`
  - `Data/sales.json`
- Files are created automatically if missing

---

## Medicine Fields

- Full Name
- Notes
- Expiry Date
- Quantity
- Price *(rounded to 2 decimal places on the server)*
- Brand

---

## UI Behavior

### Inventory Tab
- Displays a table with:
  - Full Name
  - Brand
  - Expiry
  - Quantity
  - Price
- **Notes are not shown in the grid** (only used in add/edit form)

### Row Highlighting
- 🔴 **Red** → Expiry date is less than 30 days (or already expired)
- 🟡 **Yellow** → Quantity < 10 *(only if not already red)*

### Search
- Filters medicines by name
- Uses debounced API call:
