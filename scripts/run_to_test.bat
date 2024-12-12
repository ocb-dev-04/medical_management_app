@echo off

@REM gateway
start powershell.exe -NoExit -Command "dotnet run --project ../src/back/gateway/Doctor.Management.Gateway"

@REM services
start powershell.exe -NoExit -Command "dotnet run --project ../src/back/services/auth//Services.Auth.Api"
start powershell.exe -NoExit -Command "dotnet run --project ../src/back/services/diagnosis/Service.Diagnoses.Api"
start powershell.exe -NoExit -Command "dotnet run --project ../src/back/services/doctor/Services.Doctors.Api"
start powershell.exe -NoExit -Command "dotnet run --project ../src/back/services/patient/Services.Patient.Api"

exit
