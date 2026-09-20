# CrossApp 
Наскрізний проєкт з крос-платформного програмування. 
Предметна область: Склад. 
Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).
Призначення: облік залишків товарів по партіях.
## Структура рішення
* `src/Core/` — бібліотека класів (Class Library), що містить спільну логіку, збір системної інформації, а згодом міститиме доменну модель. Не залежить від інших проєктів.
* `src/Cli/` — консольний застосунок, точка входу. Має `ProjectReference` на `Core` і відповідає виключно за взаємодію з користувачем та вивід даних.
## Каталоги для майбутньої логіки в Core:
  * `src/Core/Dto/` — record-типи формату даних (тиждень 3)
  * `src/Core/Domain/` — сутності з поведінкою та інваріантами (тиждень 4)
  * `src/Core/Storage/` — реалізації сховищ (тиждень 5)
## Запуск 
* dotnet build 
* dotnet run --project src/Cli 
* dotnet run --project src/Cli -- --json
* dotnet publish src/Cli -c Release -r win-x64 --self-contained true
* dotnet publish src/Cli -c Release -r win-x64 --self-contained false
## Середовище 
.NET SDK 10.0, Windows 11 x64
## Порівняно розмір каталогів:
* windows-x64 : 76.66 MB
* linux-x64 : 78.79 MB
## Порівняно запуски:
* Локальний запуск: Microsoft Windows 10.0.26200
* Запуск в контейнері : Debian GNU/Linux 12 (bookworm)
## Порівняння режимів публікації
| RID | режим | розмір | чи потрібен встановлений runtime |
| :--- | :--- | :--- | :--- |
| win-x64 | self-contained | 76.68 MB | ні |
| win-x64 | framework-dependent | 0.19 MB | так |
| linux-x64 | self-contained | 78.81 MB | ні |
## Різниця між Framework-dependent та Self-contained:
* Framework-dependent publish – публікація, що містить код застосунку та його залежності,
але не містить середовища виконання .NET. Тому на комп’ютері користувача має бути
встановлений .NET Runtime відповідної версії.
* Self-contained publish – публікація, до якої входить також .NET Runtime. Такий застосунок
може працювати на комп’ютері без попередньо встановленого .NET, але займає значно
більше місця та створюється для конкретного ідентифікатора платформи (RID), наприклад
win-x64.

