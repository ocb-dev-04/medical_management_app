echo Creating migration...
dotnet ef migrations add Doctors_Migration --context AppDbContext --startup-project ../../src/back/services/doctor/Services.Doctors.Api --project ../../src/back/services/doctor/Services.Doctors.Persistence -o Migrations

echo Updating database...
dotnet ef database update --context AppDbContext --startup-project ../../src/back/services/doctor/Services.Doctors.Api --project ../../src/back/services/doctor/Services.Doctors.Persistence