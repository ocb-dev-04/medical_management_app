echo Creating migration...
dotnet ef migrations add Patients_Migration --context AppDbContext --startup-project ../../src/back/services/patient/Services.Patients.Api --project ../../src/back/services/patient/Services.Patients.Persistence -o Migrations

echo Updating database...
dotnet ef database update --context AppDbContext --startup-project ../../src/back/services/patient/Services.Patients.Api --project ../../src/back/services/patient/Services.Patients.Persistence